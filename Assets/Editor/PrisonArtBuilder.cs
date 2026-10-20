using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ObstacleCourse;

// A visual trial on the first sector and the tutorial's prison walls.
// Art is placed around the existing colliders; no gameplay objects are recreated.
public static class PrisonArtBuilder
{
    private const string Licensed = "Assets/Art/Licensed/ModernInteriors/";
    private const string Generated = "Assets/Art/Prison/Generated/";
    private const string Sprites = "Assets/Art/Prison/Sprites/";
    private static Material material;
    private static Sprite square;

    public static void Apply(GameController game)
    {
        if (!File.Exists(Licensed + "RoomBuilder.png") || !File.Exists(Generated + "Prisoner.png"))
        {
            Debug.LogWarning("ARTE: importe Modern Interiors Free e as artes geradas antes de reconstruir o teste visual.");
            return;
        }
        Directory.CreateDirectory(Sprites);
        material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Prototype/Unlit.mat");
        square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/Square.png");
        var rooms = Texture(Licensed + "RoomBuilder.png", 512);
        var level = game.levels[0];
        var root = new GameObject("Arte — prisão (teste)").transform;
        root.SetParent(level.transform, false);

        var floor = Slice(rooms, "ConcreteFloor", new Rect(192, 192, 16, 16), 24);
        // Every floor tile stays inside the original walkable bounds.
        Tiled("Piso de concreto", root, floor, new Vector2(-10.675f, 0), new Vector2(8.35f, 12), -8);
        // Sparse joints make large concrete slabs readable at the game's normal zoom.
        for (float x = -13.8f; x < -6.5f; x += 1.05f)
            Solid("Junta vertical", root, new Vector2(x, 0), new Vector2(.025f, 12), "#829092", -7);
        for (float y = -5; y < 6; y += 1)
            Solid("Junta horizontal", root, new Vector2(-10.675f, y), new Vector2(8.35f, .025f), "#829092", -7);
        // Thin strips describe the maintenance lane without introducing obstacles.
        Solid("Faixa lateral esquerda", root, new Vector2(-14.35f, 0), new Vector2(.06f, 11.7f), "#D0AC58", -7);
        Solid("Faixa lateral direita", root, new Vector2(-7.05f, -1.2f), new Vector2(.06f, 9.2f), "#D0AC58", -7);

        DressWalls(level, root);

        var start = level.transform.Find("Área inicial");
        start.GetComponent<SpriteRenderer>().enabled = false;
        level.transform.Find("INÍCIO").gameObject.SetActive(false);

        var rotor = level.rotors[0];
        var bar = rotor.transform.Find("Barra").GetComponent<SpriteRenderer>();
        var beam = TightSprite(Generated + "Rotor.png", "IndustrialArm", 128);
        if (beam != null)
        {
            bar.enabled = false;
            rotor.transform.Find("Eixo").GetComponent<SpriteRenderer>().enabled = false;
            var graphic = Draw("Braço mecânico", rotor.transform, beam, rotor.transform.position, new Vector2(3.6f, .32f), 4);
            graphic.transform.localPosition = new Vector3(0, 0, -.04f);
            graphic.transform.localRotation = Quaternion.identity;
        }
        var block = level.blocks[0];
        var gate = TightSprite(Generated + "Slider.png", "SlidingGate", 64);
        if (gate != null)
        {
            block.GetComponent<SpriteRenderer>().enabled = false;
            var graphic = Draw("Portão de segurança", block.transform, gate, block.transform.position, Vector2.one, 4);
            graphic.transform.localPosition = new Vector3(0, 0, -.04f);
        }
        // Rails and sweep markings remain visible above the new floor.
        var rail = level.transform.Find("Bloco superior trilho").GetComponent<SpriteRenderer>();
        rail.color = new Color(.27f, .28f, .34f);
        foreach (Transform child in level.transform)
            if (child.name == "Barra giratória trajetória")
                child.GetComponent<SpriteRenderer>().color = new Color(.48f, .34f, .20f);
        var rotorLabel = level.transform.Find("BARRA GIRATÓRIA").GetComponent<TextMesh>();
        rotorLabel.text = "BRAÇO GIRATÓRIO";
        rotorLabel.color = Color.white;
        rotorLabel.GetComponent<MeshRenderer>().sortingOrder = -4;
        Solid("Placa do braço giratório", root, new Vector2(-11, 1.8f), new Vector2(3.5f, .45f), "#363A48", -5);

        DressPlayer(game);
        // Below the controls, clear of the existing hint panel.
        Label("Piso: LimeZu · Modern Interiors Free  |  Personagem, máquinas e grades: arte gerada", root,
            new Vector2(0, -8.35f), .028f, new Color(.65f, .72f, .78f));
        Debug.Log("ARTE: primeiro setor do tutorial usa Modern Interiors Free + sprites gerados.");
    }

    private static void DressWalls(CourseLevel level, Transform root)
    {
        var bars = TightSprite(Generated + "CellBars.png", "CellBars", 128);
        if (bars == null) return;
        foreach (var collider in level.GetComponentsInChildren<BoxCollider2D>())
        {
            if (collider.isTrigger || collider.gameObject.layer != 8) continue;
            collider.GetComponent<SpriteRenderer>().enabled = false;
            var highlight = level.transform.Find(collider.name + " destaque");
            if (highlight != null) highlight.GetComponent<SpriteRenderer>().enabled = false;
            // Match the solid collider, rather than maintain a second set of wall coordinates.
            Vector2 size = Vector2.Scale(collider.size, collider.transform.lossyScale);
            Vector2 center = collider.transform.TransformPoint(collider.offset);
            Solid(collider.name + " base", root, center, size, "#292C39", 1);
            bool vertical = size.y > size.x;
            float thickness = vertical ? size.x : size.y;
            float length = vertical ? size.y : size.x;
            float scale = thickness / bars.bounds.size.y;
            var graphic = Draw(collider.name + " grades", root, bars, center, bars.bounds.size, 2);
            graphic.drawMode = SpriteDrawMode.Tiled;
            graphic.size = new Vector2(length / scale, bars.bounds.size.y);
            graphic.transform.localScale = new Vector3(scale, scale, 1);
            graphic.transform.rotation = Quaternion.Euler(0, 0, vertical ? 90 : 0);
        }
    }

    private static void DressPlayer(GameController game)
    {
        var atlas = Texture(Generated + "Prisoner.png", 128);
        var columns = OccupiedBands(atlas, true);
        var rows = OccupiedBands(atlas, false);
        if (columns.Count != 4 || rows.Count != 4)
            throw new InvalidOperationException("Prisoner.png precisa conter quatro linhas e quatro colunas separadas por transparência.");
        var frames = new Sprite[16];
        // Detect transparent gutters: generated sheets are not guaranteed to have equal cells.
        // Equal quarters would cut feet and leak them into the following direction's frames.
        for (int row = 0; row < 4; row++)
        for (int column = 0; column < 4; column++)
        {
            Rect region = new Rect(columns[column].x, rows[row].x, columns[column].y, rows[row].y);
            Rect bounds = AlphaBounds(atlas, region);
            frames[row * 4 + column] = Slice(atlas, "Prisoner_" + row + "_" + column, bounds, 24,
                new Vector2(.5f, .28f));
        }
        var visual = new GameObject("Prisioneiro", typeof(SpriteRenderer));
        visual.transform.SetParent(game.player.transform, false);
        // Cancel the original square's .7 scale: collider dimensions stay exactly the same.
        visual.transform.localScale = Vector3.one / .7f;
        visual.transform.localPosition = new Vector3(0, 0, -.02f);
        var renderer = visual.GetComponent<SpriteRenderer>();
        renderer.sprite = frames[0]; renderer.sortingOrder = 5; renderer.sharedMaterial = material;
        var animation = game.player.gameObject.AddComponent<PrisonerVisual>();
        animation.game = game; animation.prototype = game.player.visual;
        animation.character = renderer; animation.frames = frames;
    }

    private static Texture2D Texture(string path, int maxSize)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Default;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.isReadable = true;
        importer.alphaIsTransparency = true;
        importer.maxTextureSize = maxSize;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // Rectangles use image coordinates (origin at top left).
    private static Sprite Slice(Texture2D texture, string name, Rect rect, float ppu, Vector2? pivot = null)
    {
        string path = Sprites + name + ".asset";
        var sprite = Sprite.Create(texture, new Rect(rect.x, texture.height - rect.y - rect.height, rect.width, rect.height),
            pivot ?? new Vector2(.5f, .5f), ppu, 0, SpriteMeshType.FullRect);
        sprite.name = name;
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(sprite, existing);
            UnityEngine.Object.DestroyImmediate(sprite);
            EditorUtility.SetDirty(existing);
            return existing;
        }
        AssetDatabase.CreateAsset(sprite, path);
        return sprite;
    }

    private static Rect AlphaBounds(Texture2D texture, Rect region)
    {
        int x0 = (int)region.xMax, y0 = (int)region.yMax, x1 = (int)region.x, y1 = (int)region.y;
        for (int y = (int)region.y; y < region.yMax; y++)
        for (int x = (int)region.x; x < region.xMax; x++)
        {
            if (texture.GetPixel(x, texture.height - 1 - y).a < .5f) continue;
            x0 = Mathf.Min(x0, x); y0 = Mathf.Min(y0, y);
            x1 = Mathf.Max(x1, x + 1); y1 = Mathf.Max(y1, y + 1);
        }
        return x1 > x0 && y1 > y0 ? new Rect(x0, y0, x1 - x0, y1 - y0) : region;
    }

    private static List<Vector2Int> OccupiedBands(Texture2D texture, bool columns)
    {
        var bands = new List<Vector2Int>();
        int count = columns ? texture.width : texture.height;
        int across = columns ? texture.height : texture.width;
        int start = -1;
        for (int i = 0; i <= count; i++)
        {
            bool occupied = false;
            if (i < count)
                for (int j = 0; j < across; j++)
                {
                    int x = columns ? i : j;
                    int y = columns ? j : i;
                    if (texture.GetPixel(x, texture.height - 1 - y).a >= .5f) { occupied = true; break; }
                }
            if (occupied && start < 0) start = i;
            if (!occupied && start >= 0)
            {
                bands.Add(new Vector2Int(start, i - start));
                start = -1;
            }
        }
        return bands;
    }

    private static Sprite TightSprite(string path, string name, int maxSize)
    {
        if (!File.Exists(path)) return null;
        var texture = Texture(path, maxSize);
        return Slice(texture, name, AlphaBounds(texture, new Rect(0, 0, texture.width, texture.height)), 24);
    }

    private static SpriteRenderer Draw(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size, int order)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(position.x, position.y, -order * .01f);
        // World dimensions are independent of scaled physics parents.
        go.transform.localScale = new Vector3(size.x / sprite.bounds.size.x / parent.lossyScale.x,
            size.y / sprite.bounds.size.y / parent.lossyScale.y, 1);
        var renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite; renderer.sortingOrder = order; renderer.sharedMaterial = material;
        return renderer;
    }

    private static void Tiled(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size, int order)
    {
        var renderer = Draw(name, parent, sprite, position, sprite.bounds.size, order);
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = size;
    }

    private static void Solid(string name, Transform parent, Vector2 position, Vector2 size, string color, int order)
    {
        var renderer = Draw(name, parent, square, position, size, order);
        ColorUtility.TryParseHtmlString(color, out var tint);
        renderer.color = tint;
    }

    private static void Label(string value, Transform parent, Vector2 position, float size, Color? color = null)
    {
        var go = new GameObject(value, typeof(TextMesh));
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(position.x, position.y, .05f);
        var text = go.GetComponent<TextMesh>();
        text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 64; text.characterSize = size; text.anchor = TextAnchor.MiddleCenter;
        text.color = color ?? new Color(.22f, .23f, .30f);
        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = text.font.material; renderer.sortingOrder = -5;
    }
}
