using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class DungeonCrawlerTileGenerator : EditorWindow
{
    public Material floorMaterial;
    public Material ceilingMaterial;
    public Material wallMaterial;
    public int TileSize = 3;
    public float TileHeight = 3;

    [MenuItem("Tools/Dungeon Crawler Tile Prefab Generator")]
    public static void ShowWindow()
    {
        GetWindow<DungeonCrawlerTileGenerator>("Tile Prefab Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("/Dungeon Crawler Tile Prefab Generator", EditorStyles.boldLabel);
        floorMaterial = (Material)EditorGUILayout.ObjectField("Floor Material", floorMaterial, typeof(Material), false);
        ceilingMaterial = (Material)EditorGUILayout.ObjectField("Ceiling Material", ceilingMaterial, typeof(Material), false);
        wallMaterial = (Material)EditorGUILayout.ObjectField("Wall Material", wallMaterial, typeof(Material), false);
        TileSize = EditorGUILayout.IntField("Tile Size", TileSize);
        TileHeight = EditorGUILayout.FloatField("Tile Height", TileHeight);

        if (GUILayout.Button("Generate Prefabs"))
        {
            GenerateTilePrefabs();
        }
    }

    private void GenerateTilePrefabs()
    {
        if (floorMaterial == null || ceilingMaterial == null || wallMaterial == null)
        {
            Debug.LogError("All materials must be assigned.");
            return;
        }

        string basePath = $"Assets/Generated/Tiles_{TileSize}x{TileSize}x{TileHeight}/";
        string meshPath = basePath + "Meshes";
        Directory.CreateDirectory(basePath);
        Directory.CreateDirectory(meshPath);

        string[] wallLetters = { "_E", "_W", "_S", "_N" };

        for (int i = 0; i < 16; i++)
        {
            string tileName = "Tile";

            for (int wallIndex = 0; wallIndex < 4; wallIndex++)
            {
                if (((i >> wallIndex) & 1) == 1)
                {
                    tileName += wallLetters[wallIndex];
                }
            }

            GameObject tile = new GameObject(tileName);

            MeshRenderer meshRenderer = tile.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = tile.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();

            Vector3[] vertices;
            int[][] triangles;
            Vector2[] uvs;

            CreateTileMeshData(TileSize, i, out vertices, out triangles, out uvs);

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.subMeshCount = triangles.Length;

            for (int j = 0; j < triangles.Length; j++)
            {
                mesh.SetTriangles(triangles[j], j);
            }

            mesh.RecalculateNormals();

            // Save the mesh asset
            string meshAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{meshPath}/{tileName}_Mesh.asset");
            AssetDatabase.CreateAsset(mesh, meshAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Mesh meshAsset = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);

            meshFilter.sharedMesh = meshAsset;

            Material[] materials = new Material[]
            {
                floorMaterial,
                ceilingMaterial,
                wallMaterial
            };

            meshRenderer.sharedMaterials = materials;

            string prefabPath = $"{basePath}/{tileName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(tile, prefabPath);
            DestroyImmediate(tile);
        }
    }

    private void CreateTileMeshData(float size, int walls, out Vector3[] vertices, out int[][] triangles, out Vector2[] uvs)
    {
        List<Vector3> verticesList = new List<Vector3>();
        List<int> trianglesFloor = new List<int>();
        List<int> trianglesCeiling = new List<int>();
        List<int> trianglesWalls = new List<int>();
        List<Vector2> uvsList = new List<Vector2>();

        float halfSize = size / 2;

        // Floor
        verticesList.Add(new Vector3(-halfSize, 0, halfSize));
        verticesList.Add(new Vector3(halfSize, 0, halfSize));
        verticesList.Add(new Vector3(halfSize, 0, -halfSize));
        verticesList.Add(new Vector3(-halfSize, 0, -halfSize));

        trianglesFloor.AddRange(new int[] { 0, 1, 2, 0, 2, 3 });

        // Add UVs for floor
        uvsList.AddRange(new Vector2[]
        {
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(0, 0)
        });

        // Ceiling
        int vertexOffset = verticesList.Count;

        verticesList.Add(new Vector3(-halfSize, TileHeight, halfSize));
        verticesList.Add(new Vector3(halfSize, TileHeight, halfSize));
        verticesList.Add(new Vector3(halfSize, TileHeight, -halfSize));
        verticesList.Add(new Vector3(-halfSize, TileHeight, -halfSize));

        trianglesCeiling.AddRange(new int[] { vertexOffset + 0, vertexOffset + 2, vertexOffset + 1, vertexOffset + 0, vertexOffset + 3, vertexOffset + 2 });

        // Add UVs for ceiling
        uvsList.AddRange(new Vector2[]
        {
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(0, 0)
        });

        // Walls
        Vector3[] wallVertices = new Vector3[]
        {
            new Vector3(halfSize, 0, halfSize),
            new Vector3(halfSize, TileHeight, halfSize),
            new Vector3(halfSize, TileHeight, -halfSize),
            new Vector3(halfSize, 0, -halfSize),

            new Vector3(-halfSize, 0, -halfSize),
            new Vector3(-halfSize, TileHeight, -halfSize),
            new Vector3(-halfSize, TileHeight, halfSize),
            new Vector3(-halfSize, 0, halfSize),

            new Vector3(-halfSize, 0, -halfSize),
            new Vector3(-halfSize, TileHeight, -halfSize),
            new Vector3(halfSize, TileHeight, -halfSize),
            new Vector3(halfSize, 0, -halfSize),

            new Vector3(-halfSize, 0, halfSize),
            new Vector3(-halfSize, TileHeight, halfSize),
            new Vector3(halfSize, TileHeight, halfSize),
            new Vector3(halfSize, 0, halfSize),
        };

        Vector2[] wallUVs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),

            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),

            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),

            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
        };

        for (int wallIndex = 0; wallIndex < 4; wallIndex++)
        {
            if (((walls >> wallIndex) & 1) == 1)
            {
                vertexOffset = verticesList.Count;

                verticesList.AddRange(new Vector3[]
                {
                    wallVertices[wallIndex * 4 + 0],
                    wallVertices[wallIndex * 4 + 1],
                    wallVertices[wallIndex * 4 + 2],
                    wallVertices[wallIndex * 4 + 3],
                });

                if (wallIndex == 2) // South wall we need to flip the normals
                {
                    trianglesWalls.AddRange(new int[]
                    {
                        vertexOffset + 0,
                        vertexOffset + 2,
                        vertexOffset + 1,
                        vertexOffset + 2,
                        vertexOffset + 0,
                        vertexOffset + 3,
                    });
                }
                else
                {
                    trianglesWalls.AddRange(new int[]
                    {
                        vertexOffset + 0,
                        vertexOffset + 1,
                        vertexOffset + 2,
                        vertexOffset + 0,
                        vertexOffset + 2,
                        vertexOffset + 3,
                    });
                }

                uvsList.AddRange(new Vector2[]
                {
                    wallUVs[wallIndex * 4 + 0],
                    wallUVs[wallIndex * 4 + 1],
                    wallUVs[wallIndex * 4 + 2],
                    wallUVs[wallIndex * 4 + 3],
                });
            }
    
        }

        vertices = verticesList.ToArray();
        triangles = new int[][] { trianglesFloor.ToArray(), trianglesCeiling.ToArray(), trianglesWalls.ToArray() };
        uvs = uvsList.ToArray();
    }
}
