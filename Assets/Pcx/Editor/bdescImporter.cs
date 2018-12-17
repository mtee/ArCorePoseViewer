// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pcx
{
    [ScriptedImporter(1, "bdesc")]
    class BDescImporter : ScriptedImporter
    {
        #region ScriptedImporter implementation

        public enum ContainerType { Mesh, ComputeBuffer  }

        [SerializeField] ContainerType _containerType;

        public override void OnImportAsset(AssetImportContext context)
        {
          
                // Mesh container
                // Create a prefab with MeshFilter/MeshRenderer.
                var gameObject = new GameObject();
                var mesh = ImportAsMesh(context.assetPath);

                var meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;

                var meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = GetDefaultMaterial();

                context.AddObjectToAsset("prefab", gameObject);
                if (mesh != null) context.AddObjectToAsset("mesh", mesh);

                context.SetMainObject(gameObject);
        
        }

        #endregion

        #region Internal utilities

        static Material GetDefaultMaterial()
        {
            return AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Pcx/Editor/Default Point.mat"
            );
        }

        #endregion

        #region Internal data structure

        enum DataProperty {
            Invalid,
            X, Y, Z,
            R, G, B, A,
            Data8, Data16, Data32
        }

        static int GetPropertySize(DataProperty p)
        {
            switch (p)
            {
                case DataProperty.X: return 4;
                case DataProperty.Y: return 4;
                case DataProperty.Z: return 4;
                case DataProperty.R: return 1;
                case DataProperty.G: return 1;
                case DataProperty.B: return 1;
                case DataProperty.A: return 1;
                case DataProperty.Data8: return 1;
                case DataProperty.Data16: return 2;
                case DataProperty.Data32: return 4;
            }
            return 0;
        }

        class DataHeader
        {
			public uint nb_3D_points = 0;
			public uint nb_clusts = 0;
			public uint nb_non_empty_vw = 0;
			public uint nb_descriptors = 0;
        }

        class DataBody
        {
            public List<Vector3> vertices;
            public List<Color32> colors;

            public DataBody(uint vertexCount)
            {
				vertices = new List<Vector3>((int)vertexCount);
				colors = new List<Color32>((int)vertexCount);
            }

            public void AddPoint(
                float x, float y, float z,
                byte r, byte g, byte b, byte a
            )
            {
                vertices.Add(new Vector3(x, y, z));
                colors.Add(new Color32(r, g, b, a));
            }
        }

        #endregion

        #region Reader implementation

        Mesh ImportAsMesh(string path)
        {
            try
            {
				
                var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				var reader = new BinaryReader(stream);
				UInt32 nb_3D_points = reader.ReadUInt32();
				UInt32 nb_clusts = reader.ReadUInt32();
				UInt32 nb_non_empty_vw = reader.ReadUInt32();
				UInt32 nb_descriptors = reader.ReadUInt32();

				Debug.Log("nb_points is: " + nb_3D_points);
				Debug.Log("nb_clusts is: " + nb_clusts);
				Debug.Log("nb_non_empty_vw is: " + nb_non_empty_vw);
				Debug.Log("nb_descriptors is: " + nb_descriptors);

                var mesh = new Mesh();
                mesh.name = Path.GetFileNameWithoutExtension(path);

    /*            mesh.indexFormat = header.vertexCount > 65535 ?
                    IndexFormat.UInt32 : IndexFormat.UInt16;

                mesh.SetVertices(body.vertices);
                mesh.SetColors(body.colors);

                mesh.SetIndices(
                    Enumerable.Range(0, header.vertexCount).ToArray(),
                    MeshTopology.Points, 0
                );
*/
                mesh.UploadMeshData(true);
                return mesh;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed importing " + path + ". " + e.Message);
                return null;
            }
        }
			

        DataBody ReadDataBody(DataHeader header, BinaryReader reader)
        {
			var data = new DataBody(header.nb_3D_points);

            float x = 0, y = 0, z = 0;
            Byte r = 255, g = 255, b = 255, a = 255;

			for (var i = 0; i < header.nb_3D_points; i++)
            {
                x = reader.ReadSingle(); break;
                y = reader.ReadSingle(); break;
                z = reader.ReadSingle(); break;
                r = reader.ReadByte(); break;
                g = reader.ReadByte(); break;
                b = reader.ReadByte(); break;
               //         case DataProperty.A: a = reader.ReadByte(); break;

               //         case DataProperty.Data8: reader.ReadByte(); break;
                //        case DataProperty.Data16: reader.BaseStream.Position += 2; break;
                //        case DataProperty.Data32: reader.BaseStream.Position += 4; break;
				data.AddPoint(x, y, z, r, g, b, a);
             }
            return data;
        }
    }

    #endregion
}
