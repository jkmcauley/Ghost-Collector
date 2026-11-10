using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class CemeteryBuilder
{
    const string ScenePath = "Assets/Scenes/Cemetery.unity";
    const string TileRoot = "Assets/RafaelMatos/ERW - Cemetery V 1.6/Tilesets/Rule Tile/";
    const string PropRoot = "Assets/RafaelMatos/ERW - Cemetery V 1.6/Props/props by individual sprites/";
    const string HoleSheet = "Assets/RafaelMatos/ERW - Cemetery V 1.6/Tilesets/hole-1.png";
    const string StairsSheet = "Assets/RafaelMatos/ERW - Cemetery V 1.6/Tilesets/stairs.png";
    const string FogSprite = "Assets/RafaelMatos/ERW - Cemetery V 1.6/Props/animated/FOG-1.png";
    const string CandlePrefab = "Assets/RafaelMatos/ERW - Cemetery V 1.6/Props/prefabs/Candle.prefab";
    const string CrowPrefab = "Assets/RafaelMatos/Characters/Crow.prefab";
    const string BatPrefab = "Assets/RafaelMatos/Characters/Bat1.prefab";
    const string BatPrefab2 = "Assets/RafaelMatos/Characters/Bat2.prefab";
    const string InsectPrefab = "Assets/RafaelMatos/ERW - Cemetery V 1.6/Props/prefabs/LightInsects.prefab";
    const string VersionKey = "GraveyardGuard.CemeteryLayoutVersion";
    const string CameraKey = "GraveyardGuard.PlayCameraVersion";
    const int LayoutVersion = 7;
    const int CameraVersion = 1;
    const float PlayOrtho = 2f;

    const int Size = 96;
    const int HoleCenterCol = 6;
    const int HoleCenterRow = 11;
    const float Cell = 0.32f;

    static readonly string[] Graves =
    {
        "tombstone - 1.png",
        "tombstone - 3.png",
        "tombstone - 5.png",
        "tombstone - cross - 1.png",
        "tombstone - cross - 3.png",
        "tombstone - tall - 1.png",
        "tombstone - tall - 2.png",
        "tombstone - 8.png",
    };

    static readonly string[] Trees =
    {
        "tree - darker - 1.png",
        "tree - darker - 2.png",
        "tree - darker - 3.png",
        "tree - darker - 4.png",
        "tree - darker - 5.png",
        "tree - darker - 6.png",
    };

    static readonly Vector3Int[] GravePlots =
    {
        new Vector3Int(26, 33, 15),
        new Vector3Int(56, 33, 15),
        new Vector3Int(33, 53, 15),
        new Vector3Int(33, 29, 15),
        new Vector3Int(6, 6, 14),
        new Vector3Int(76, 6, 14),
        new Vector3Int(6, 76, 14),
        new Vector3Int(76, 76, 14),
    };

    static bool building;
    static int graveIndex;

    [MenuItem("Graveyard Guard/Build Cemetery")]
    public static void BuildFromMenu()
    {
        Build();
        EditorPrefs.SetInt(VersionKey, LayoutVersion);
    }

    [InitializeOnLoadMethod]
    static void BuildOnLoad()
    {
        EditorApplication.delayCall += () => EditorApplication.update += WaitUntilReady;
    }

    static void WaitUntilReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        EditorApplication.update -= WaitUntilReady;
        if (EditorPrefs.GetInt(VersionKey, 0) != LayoutVersion || !File.Exists(ScenePath))
        {
            Build();
            EditorPrefs.SetInt(VersionKey, LayoutVersion);
        }

        if (EditorPrefs.GetInt(CameraKey, 0) == CameraVersion)
            return;

        FramePlayCamera();
        EditorPrefs.SetInt(CameraKey, CameraVersion);
    }

    static void FramePlayCamera()
    {
        Scene scene = EditorSceneManager.GetSceneByPath(ScenePath);
        if (!scene.IsValid() || !scene.isLoaded)
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        EditorSceneManager.SetActiveScene(scene);
        Camera camera = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.CompareTag("MainCamera"))
                camera = root.GetComponent<Camera>();
        }

        if (camera == null)
            return;

        camera.orthographic = true;
        camera.orthographicSize = PlayOrtho;
        camera.transform.position = new Vector3((48.5f) * Cell, (40.5f) * Cell, -10f);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void Build()
    {
        if (building)
            return;

        building = true;
        graveIndex = 0;
        try
        {
            Scene scene = OpenCemeteryScene();
            EditorSceneManager.SetActiveScene(scene);

            var grass = LoadTile("grass1 to transp.asset");
            var pathTile = LoadTile("stone ground-path to transp.asset");
            var dirt = LoadTile("dirt2 to transp.asset");
            var platform = LoadTile("platform ground-dark to light.asset");
            var fence = LoadTile("fences1.asset");

            var gridObject = new GameObject("Grid");
            var grid = gridObject.AddComponent<Grid>();
            grid.cellSize = new Vector3(Cell, Cell, 0f);

            Tilemap ground = CreateTilemap(gridObject.transform, "Ground", -5);
            Tilemap dirtMap = CreateTilemap(gridObject.transform, "Dirt", -4);
            Tilemap pathMap = CreateTilemap(gridObject.transform, "Path", -3);
            Tilemap platformMap = CreateTilemap(gridObject.transform, "Platform", -2);
            Tilemap fenceMap = CreateTilemap(gridObject.transform, "Fence", 0);
            fenceMap.gameObject.AddComponent<TilemapCollider2D>();

            var props = new GameObject("Props").transform;
            FillAll(ground, grass, 0, 0, Size, Size);

            var pathCells = new List<Vector3Int>();
            var platformCells = new List<Vector3Int>();
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    if (OnPath(x, y))
                        pathCells.Add(new Vector3Int(x, y, 0));
                    if (OnPlatform(x, y))
                        platformCells.Add(new Vector3Int(x, y, 0));
                }
            }

            Paint(pathMap, pathCells, pathTile);
            Paint(platformMap, platformCells, platform);

            for (int x = 1; x < Size - 1; x++)
            {
                if (x < 46 || x > 50)
                    fenceMap.SetTile(new Vector3Int(x, 1, 0), fence);
                if (!OnPath(x, Size - 2))
                    fenceMap.SetTile(new Vector3Int(x, Size - 2, 0), fence);
            }

            for (int y = 1; y < Size - 1; y++)
            {
                if (OnPath(1, y))
                    continue;
                fenceMap.SetTile(new Vector3Int(1, y, 0), fence);
                fenceMap.SetTile(new Vector3Int(Size - 2, y, 0), fence);
            }

            PlacePlots(props, grid, dirtMap, dirt);
            PlaceDressing(props, grid);
            PlaceStairs(props, grid);
            PlaceVortex(grid);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = PlayOrtho;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.07f, 0.12f, 1f);
            camera.transform.position = new Vector3(48.5f * Cell, 40.5f * Cell, -10f);
            cameraObject.AddComponent<AudioListener>();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Cemetery scene saved to " + ScenePath);
        }
        finally
        {
            building = false;
        }
    }

    static void PlacePlots(Transform props, Grid grid, Tilemap dirtMap, TileBase dirt)
    {
        foreach (Vector3Int plot in GravePlots)
        {
            int size = plot.z;
            Fill(dirtMap, dirt, plot.x, plot.y, size, size);
            bool firstCandle = true;
            for (int oy = 1; oy < size - 1; oy += 2)
            {
                for (int ox = 1; ox < size - 1; ox += 2)
                {
                    int x = plot.x + ox;
                    int y = plot.y + oy;
                    if (OnPath(x, y) || OnPlatform(x, y))
                        continue;
                    Place(props, grid, Graves[graveIndex++ % Graves.Length], x, y, true);
                    if ((ox + oy) % 4 == 2)
                        Place(props, grid, "candle 1 - 1.png", x, y, false);
                    if (firstCandle)
                    {
                        PlacePrefab(props, grid, CandlePrefab, x, y);
                        firstCandle = false;
                    }
                }
            }

            int skullX = plot.x + 2;
            int skullY = plot.y + size - 3;
            if (!OnPath(skullX, skullY) && !OnPlatform(skullX, skullY))
                Place(props, grid, "skull - color scheme 1 - 3.png", skullX, skullY, true);
            int spikeX = plot.x + size - 3;
            int spikeY = plot.y + 2;
            if (!OnPath(spikeX, spikeY) && !OnPlatform(spikeX, spikeY))
                Place(props, grid, "skull in a spike_" + (graveIndex % 8) + ".png", spikeX, spikeY, true);
        }
    }

    static void PlaceDressing(Transform props, Grid grid)
    {
        Place(props, grid, "chapel.png", 12, 78, true);
        Place(props, grid, "crypt - 1.png", 20, 16, true);
        Place(props, grid, "crypt - 3.png", 78, 18, true);
        Place(props, grid, "crypt - 5.png", 74, 70, true);

        int tree = 0;
        int[] treeSpots =
        {
            10, 18, 26, 14, 34, 42, 80, 24, 14, 44, 84, 62, 24, 86, 68, 82, 86, 40, 40, 14,
        };
        for (int i = 0; i < treeSpots.Length; i += 2)
        {
            int x = treeSpots[i];
            int y = treeSpots[i + 1];
            Place(props, grid, Trees[tree % Trees.Length], x, y, true);
            Place(props, grid, "tree - darker - roots -" + ((tree % 6) + 1) + ".png", x, y, true);
            tree++;
        }

        int[] lightTrees = { 18, 36, 62, 30, 30, 80, 82, 70, 8, 52, 72, 12, 16, 60, 88, 56 };
        for (int i = 0; i < lightTrees.Length; i += 2)
        {
            int x = lightTrees[i];
            int y = lightTrees[i + 1];
            Place(props, grid, "tree " + ((i / 2) % 6 + 1) + ".png", x, y, true);
            Place(props, grid, "tree - roots " + ((i / 2) % 6 + 2) + ".png", x, y, true);
        }

        Place(props, grid, "rocks - color scheme 1 - 4.png", 22, 28, true);
        Place(props, grid, "rocks - color scheme 1 - 12.png", 70, 26, true);
        Place(props, grid, "rocks - color scheme 2 - 8.png", 32, 70, true);
        Place(props, grid, "rocks - color scheme 4 - 14.png", 60, 80, true);
        Place(props, grid, "skull - color scheme 4 - 1.png", 26, 24, true);
        Place(props, grid, "skull - color scheme 5 - 4.png", 66, 62, true);
        Place(props, grid, "skull in a spike_2.png", 18, 40, true);
        Place(props, grid, "skull - color scheme 2 - 6.png", 38, 36, true);
        Place(props, grid, "skull - color scheme 3 - 8.png", 60, 40, true);
        Place(props, grid, "skull - color scheme 5 - 11.png", 42, 58, true);
        Place(props, grid, "skull in a spike_8.png", 34, 46, true);
        Place(props, grid, "skull in a spike_14.png", 58, 34, true);

        Place(props, grid, "crypt details - statue 1.png", 24, 16, true);
        Place(props, grid, "crypt details - vase 1.png", 23, 14, true);
        Place(props, grid, "crypt details - statue 3.png", 74, 20, true);
        Place(props, grid, "crypt details - vase 2.png", 76, 16, true);
        Place(props, grid, "crypt details - statue 5.png", 70, 72, true);
        Place(props, grid, "crypt details - cross.png", 18, 74, true);

        Place(props, grid, "pillar - shorter - 1.png", 10, 30, true);
        Place(props, grid, "pillar - shorter - dirty - 2.png", 20, 50, true);
        Place(props, grid, "pillar - shorter - 4.png", 40, 80, true);
        Place(props, grid, "pillar - broken - 1.png", 88, 30, true);
        Place(props, grid, "pillar - broken - 2.png", 60, 18, true);
        Place(props, grid, "pillar - broken - 3.png", 8, 70, true);

        Place(props, grid, "pillar - 1.png", 45, 2, true);
        Place(props, grid, "pillar - 1.png", 51, 2, true);
        Place(props, grid, "gateway arc 1- to be put on top of a pillar-column.png", 48, 4, true);
        Place(props, grid, "little house - 1.png", 84, 72, true);
        Place(props, grid, "little house - 2.png", 84, 28, true);

        PlacePrefab(props, grid, BatPrefab, 44, 32);
        PlacePrefab(props, grid, BatPrefab2, 62, 48);
        PlacePrefab(props, grid, BatPrefab, 36, 56);
        PlacePrefab(props, grid, InsectPrefab, 45, 30);
        PlacePrefab(props, grid, InsectPrefab, 63, 62);

        Place(props, grid, "lamp post - color scheme 1 - lit - 1.png", 44, 28, true);
        Place(props, grid, "lamp post - color scheme 1 - lit - 3.png", 52, 36, true);
        Place(props, grid, "lamp post - color scheme 1 - lit - 2.png", 28, 32, true);
        Place(props, grid, "lamp post - color scheme 1 - lit - 4.png", 62, 60, true);
        Place(props, grid, "lamp post - color scheme 2 - lit - 1.png", 30, 68, true);
        Place(props, grid, "lamp post - color scheme 1 - lit - 5.png", 62, 44, true);

        PlaceCenterSprite(props, grid, FogSprite, 18, 34, 20);
        PlaceCenterSprite(props, grid, FogSprite, 46, 70, 20);
        PlaceCenterSprite(props, grid, FogSprite, 62, 26, 20);
        PlaceCenterSprite(props, grid, FogSprite, 24, 84, 20);

        PlacePrefab(props, grid, CrowPrefab, 14, 80);
        PlacePrefab(props, grid, CrowPrefab, 12, 20);
        PlacePrefab(props, grid, CrowPrefab, 82, 26);
        PlacePrefab(props, grid, CrowPrefab, 26, 86);
    }

    static void PlaceStairs(Transform props, Grid grid)
    {
        var sprites = SpritesByCell(StairsSheet);
        int[] cols = { 0, 1, 2 };
        for (int i = 0; i < cols.Length; i++)
        {
            if (!sprites.TryGetValue(new Vector2Int(cols[i], 8), out Sprite sprite))
                continue;
            var piece = new GameObject("stairs");
            piece.transform.SetParent(props, false);
            piece.transform.position = grid.GetCellCenterWorld(new Vector3Int(47 + i, 38, 0));
            var renderer = piece.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 3;
        }
    }

    static void PlaceVortex(Grid grid)
    {
        var vortex = new GameObject("Vortex");
        vortex.transform.position = grid.GetCellCenterWorld(new Vector3Int(Size / 2, Size / 2, 0));
        var trigger = vortex.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 1.2f;

        var sprites = SpritesByCell(HoleSheet);
        int originX = Size / 2 - HoleCenterCol;
        int originY = Size / 2 - HoleCenterRow;
        for (int row = 9; row <= 13; row++)
        {
            for (int col = 0; col <= 12; col++)
            {
                if (!sprites.TryGetValue(new Vector2Int(col, row), out Sprite sprite))
                    continue;

                var piece = new GameObject("pit");
                piece.transform.SetParent(vortex.transform, false);
                piece.transform.position = grid.GetCellCenterWorld(new Vector3Int(originX + col, originY + row, 0));
                var renderer = piece.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = 2;
            }
        }
    }

    static bool OnPath(int x, int y)
    {
        bool vertical = x >= 47 && x <= 49 && y >= 2 && y < Size - 2;
        bool horizontal = y >= 47 && y <= 49 && x >= 2 && x < Size - 2;
        bool fromChapel = y >= 64 && y <= 65 && x >= 22 && x <= 49;
        return vertical || horizontal || fromChapel;
    }

    static bool OnPlatform(int x, int y)
    {
        return x >= 41 && x <= 55 && y >= 44 && y <= 52;
    }

    static Dictionary<Vector2Int, Sprite> SpritesByCell(string sheet)
    {
        var sprites = new Dictionary<Vector2Int, Sprite>();
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(sheet))
        {
            if (asset is Sprite sprite)
                sprites[new Vector2Int((int)sprite.rect.x / 32, (int)sprite.rect.y / 32)] = sprite;
        }

        return sprites;
    }

    static Scene OpenCemeteryScene()
    {
        Scene scene = EditorSceneManager.GetSceneByPath(ScenePath);
        if (!scene.IsValid() || !scene.isLoaded)
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

        foreach (GameObject root in scene.GetRootGameObjects())
            Object.DestroyImmediate(root);

        return scene;
    }

    static TileBase LoadTile(string fileName)
    {
        var tile = AssetDatabase.LoadAssetAtPath<TileBase>(TileRoot + fileName);
        if (tile == null)
            throw new FileNotFoundException("Missing rule tile " + fileName);
        return tile;
    }

    static Tilemap CreateTilemap(Transform parent, string name, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var map = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;
        return map;
    }

    static void Paint(Tilemap map, List<Vector3Int> cells, TileBase tile)
    {
        var tiles = new TileBase[cells.Count];
        for (int i = 0; i < tiles.Length; i++)
            tiles[i] = tile;
        map.SetTiles(cells.ToArray(), tiles);
    }

    static void FillAll(Tilemap map, TileBase tile, int x, int y, int width, int height)
    {
        var positions = new Vector3Int[width * height];
        var tiles = new TileBase[width * height];
        int i = 0;
        for (int yy = y; yy < y + height; yy++)
        {
            for (int xx = x; xx < x + width; xx++)
            {
                positions[i] = new Vector3Int(xx, yy, 0);
                tiles[i] = tile;
                i++;
            }
        }

        map.SetTiles(positions, tiles);
    }

    static void Fill(Tilemap map, TileBase tile, int x, int y, int width, int height)
    {
        var positions = new Vector3Int[width * height];
        var tiles = new TileBase[width * height];
        int i = 0;
        for (int yy = y; yy < y + height; yy++)
        {
            for (int xx = x; xx < x + width; xx++)
            {
                if (OnPath(xx, yy) || OnPlatform(xx, yy))
                {
                    positions[i] = new Vector3Int(xx, yy, 0);
                    tiles[i] = null;
                }
                else
                {
                    positions[i] = new Vector3Int(xx, yy, 0);
                    tiles[i] = tile;
                }

                i++;
            }
        }

        map.SetTiles(positions, tiles);
    }

    static void Place(Transform parent, Grid grid, string fileName, int cellX, int cellY, bool feetPivot)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PropRoot + fileName);
        if (sprite == null)
        {
            Debug.LogWarning("Missing prop sprite " + fileName);
            return;
        }

        var go = new GameObject(sprite.name);
        go.transform.SetParent(parent, false);
        Vector3 position = grid.GetCellCenterWorld(new Vector3Int(cellX, cellY, 0));
        if (feetPivot)
            position.y -= Cell * 0.35f;
        go.transform.position = position;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 800 + Mathf.RoundToInt(-position.y * 40f);
    }

    static void PlaceCenterSprite(Transform parent, Grid grid, string path, int cellX, int cellY, int sortingOrder)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning("Missing sprite " + path);
            return;
        }

        var go = new GameObject(sprite.name);
        go.transform.SetParent(parent, false);
        go.transform.position = grid.GetCellCenterWorld(new Vector3Int(cellX, cellY, 0));
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
    }

    static void PlacePrefab(Transform parent, Grid grid, string path, int cellX, int cellY)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning("Missing prefab " + path);
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.SetParent(parent, false);
        Vector3 position = grid.GetCellCenterWorld(new Vector3Int(cellX, cellY, 0));
        instance.transform.position = position;
        var renderer = instance.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sortingOrder = 900 + Mathf.RoundToInt(-position.y * 40f);
    }
}
