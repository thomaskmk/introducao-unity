using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ObstacleCourse;

// Presentation only. Physics objects, dimensions and gameplay settings are never modified here.
public static class PrisonArtBuilder
{
    private const string Licensed = "Assets/Art/Licensed/ModernInteriors/";
    private const string Generated = "Assets/Art/Prison/Generated/";
    private const string Sprites = "Assets/Art/Prison/Sprites/";
    private static Material material;
    private static Sprite square;

    public static void Apply(GameController game)
    {
        if (!File.Exists(Licensed + "RoomBuilder.png"))
        {
            Debug.LogWarning("ARTE: importe Modern Interiors Free antes de reconstruir o cenário da prisão.");
            return;
        }
        Directory.CreateDirectory(Sprites);
        material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Prototype/Unlit.mat");
        square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/Square.png");
        var rooms = Texture(Licensed + "RoomBuilder.png", 512);
        var floor = Slice(rooms, "ConcreteFloor", new Rect(192, 192, 16, 16), 24);
        var bars = TightSprite(Generated + "CellBars.png", "CellBars", 128);
        var beam = TightSprite(Generated + "Rotor.png", "IndustrialArm", 128);
        var gate = TightSprite(Generated + "Slider.png", "SlidingGate", 64);
        var piston = TightSprite(Generated + "Piston.png", "SecurityPiston", 128);
        var trapTile = TightSprite(Generated + "FloorTrap.png", "SecurityFloor", 64);
        var slowTexture = Texture(Generated + "SlowFloor.png", 32);
        var water = Slice(slowTexture, "FloodedFloor", new Rect(0, 0, slowTexture.width, slowTexture.height), 24);
        var prisonerFrames = CharacterFrames("Prisoner");
        var guardFrames = CharacterFrames("Guard");
        DressCharacter(game, game.player.transform, game.player.visual, prisonerFrames, "Prisioneiro");

        foreach (var level in game.levels)
        {
            var root = new GameObject("Arte — prisão").transform;
            root.SetParent(level.transform, false);
            DressFloor(level, root, floor);
            DressWalls(level, root, bars);
            DressHazards(level, root, beam, gate, piston, trapTile, water);
            DressFinish(level, root, floor);
            DressLabels(level, root);
            foreach (var enemy in level.enemies)
            {
                var animation = DressCharacter(game, enemy.transform, enemy.GetComponent<SpriteRenderer>(), guardFrames, "Guarda");
                animation.guard = enemy;
                enemy.label.characterSize = .065f;
                enemy.label.transform.position = enemy.transform.position + new Vector3(0, 1.05f, -.04f);
                enemy.label.GetComponent<MeshRenderer>().sortingOrder = 7;
                Solid("Indicador do guarda", enemy.label.transform, enemy.label.transform.position,
                    new Vector2(.38f, .42f), "#292C39", 6);
            }
            Label("Piso: LimeZu · Modern Interiors Free  |  Personagens, máquinas e grades: arte gerada", root,
                new Vector2(0, -8.35f), .028f, new Color(.65f, .72f, .78f));
        }
        Debug.Log("ARTE: prisão aplicada às duas fases, incluindo personagens e todos os perigos.");
    }

    private static void DressFloor(CourseLevel level, Transform root, Sprite floor)
    {
        // The same concrete and joint spacing as the approved first sector, extended to the arena.
        Tiled("Piso de concreto", root, floor, Vector2.zero, new Vector2(29.7f, 12), -8);
        for (float x = -13.8f; x < 14.85f; x += 1.05f)
            Solid("Junta vertical", root, new Vector2(x, 0), new Vector2(.025f, 12), "#829092", -7);
        for (float y = -5; y < 6; y += 1)
            Solid("Junta horizontal", root, new Vector2(0, y), new Vector2(29.7f, .025f), "#829092", -7);
        Solid("Faixa lateral esquerda", root, new Vector2(-14.35f, 0), new Vector2(.06f, 11.7f), "#D0AC58", -7);
        Solid("Faixa lateral direita", root, new Vector2(14.35f, 0), new Vector2(.06f, 11.7f), "#D0AC58", -7);
        if (level.tutorial)
            Solid("Faixa do primeiro setor", root, new Vector2(-7.05f, -1.2f), new Vector2(.06f, 9.2f), "#D0AC58", -7);
        level.transform.Find("Área inicial").GetComponent<SpriteRenderer>().enabled = false;
        level.transform.Find("INÍCIO").gameObject.SetActive(false);
    }

    private static void DressWalls(CourseLevel level, Transform root, Sprite bars)
    {
        foreach (var collider in level.GetComponentsInChildren<BoxCollider2D>(true))
        {
            if (collider.isTrigger || collider.gameObject.layer != 8) continue;
            collider.GetComponent<SpriteRenderer>().enabled = false;
            var highlight = level.transform.Find(collider.name + " destaque");
            if (highlight != null) highlight.GetComponent<SpriteRenderer>().enabled = false;
            Vector2 size = WorldSize(collider);
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

    private static void DressHazards(CourseLevel level, Transform root, Sprite beam, Sprite gate,
        Sprite piston, Sprite trapTile, Sprite water)
    {
        foreach (var rotor in level.rotors)
        {
            var collider = rotor.GetComponentInChildren<BoxCollider2D>(true);
            collider.GetComponent<SpriteRenderer>().enabled = false;
            rotor.transform.Find("Eixo").GetComponent<SpriteRenderer>().enabled = false;
            var visual = Fitted("Braço mecânico", collider, beam, false, 4);
            if (rotor.oscillates) visual.color = new Color(1, .90f, .73f);
        }
        foreach (var block in level.blocks)
        {
            var collider = block.GetComponent<BoxCollider2D>();
            block.GetComponent<SpriteRenderer>().enabled = false;
            var size = WorldSize(collider);
            bool rectangular = Mathf.Abs(size.x - size.y) > .1f;
            Fitted(rectangular ? "Pistão de segurança" : "Portão de segurança", collider,
                rectangular ? piston : gate, rectangular && size.x > size.y, 4);
            level.transform.Find(block.name + " trilho").GetComponent<SpriteRenderer>().color = new Color(.27f, .28f, .34f);
        }
        foreach (var trap in level.traps)
        {
            var collider = trap.GetComponent<BoxCollider2D>();
            var size = WorldSize(collider);
            var surface = Draw("Piso de segurança", root, trapTile, trap.transform.position, trapTile.bounds.size, -5);
            float scale = size.y / trapTile.bounds.size.y;
            surface.drawMode = SpriteDrawMode.Tiled;
            surface.size = new Vector2(size.x / scale, trapTile.bounds.size.y);
            surface.transform.localScale = new Vector3(scale, scale, 1);
            var view = trap.gameObject.AddComponent<PrisonTrapVisual>();
            view.trap = trap; view.prototype = trap.GetComponent<SpriteRenderer>(); view.surface = surface;
            view.prototype.enabled = false;
            DressElectricalSignals(root, trap, size, view);
            view.Refresh();
        }
        foreach (var zone in level.slowZones)
        {
            zone.GetComponent<SpriteRenderer>().enabled = false;
            var collider = zone.GetComponent<BoxCollider2D>();
            Tiled("Área alagada", root, water, zone.transform.position, WorldSize(collider), -6);
            Border("Limite do piso lento", root, zone.transform.position, WorldSize(collider), "#6088A0", -5);
        }
        foreach (Transform child in level.transform)
            if (child.name.EndsWith(" trajetória", StringComparison.Ordinal))
                child.GetComponent<SpriteRenderer>().color = new Color(.48f, .34f, .20f);
    }

    // A child sprite inherits the exact local box and its existing rotation/scale.
    // The original physics transform and collider are never resized to accommodate art.
    private static SpriteRenderer Fitted(string name, BoxCollider2D collider, Sprite sprite, bool rotated, int order)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.SetParent(collider.transform, false);
        go.transform.localPosition = new Vector3(collider.offset.x, collider.offset.y, -.02f);
        go.transform.localRotation = Quaternion.Euler(0, 0, rotated ? 90 : 0);
        var size = rotated ? new Vector2(collider.size.y, collider.size.x) : collider.size;
        go.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1);
        var renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite; renderer.sortingOrder = order; renderer.sharedMaterial = material;
        return renderer;
    }

    private static void DressFinish(CourseLevel level, Transform root, Sprite floor)
    {
        var finish = level.GetComponentInChildren<FinishZone>(true);
        finish.GetComponent<SpriteRenderer>().enabled = false;
        var size = WorldSize(finish.GetComponent<BoxCollider2D>());
        var graphic = Tiled("Piso da saída", root, floor, finish.transform.position, size, -5);
        graphic.color = new Color(.36f, .67f, .52f);
        Border("Sinalização da saída", root, finish.transform.position, size, "#78E3AD", -4);
        var label = level.transform.Find("CHEGADA").GetComponent<TextMesh>();
        label.text = "SAÍDA";
        label.GetComponent<MeshRenderer>().sortingOrder = -3;
    }

    private static void DressLabels(CourseLevel level, Transform root)
    {
        // The floor's live status is the only instruction printed in the arena.
        // Explanations live in the proximity guide, not permanent signs.
        foreach (var trap in level.traps)
        {
            if (trap.label == null) continue;
            Solid("Estado da corrente", root, trap.label.transform.position, new Vector2(5.3f, .42f), "#292C39", -4);
            trap.label.GetComponent<MeshRenderer>().sortingOrder = -3;
            trap.Refresh();
        }
    }

    private static void DressElectricalSignals(Transform root, CycleTrap trap, Vector2 size, PrisonTrapVisual view)
    {
        Vector2 center = trap.transform.position;
        // Every mark stays inside the existing damage rectangle.
        Solid("Base isolante", root, center, size, "#252C38", -6);
        for (float x = -size.x / 2; x < size.x / 2; x += .3f)
        {
            float width = Mathf.Min(.3f, size.x / 2 - x);
            foreach (float side in new[] {-1f, 1f})
                Solid("Faixa de alta tensão", root, center + new Vector2(x + width / 2, side * (size.y / 2 - .06f)),
                    new Vector2(width, .12f), Mathf.RoundToInt((x + size.x / 2) / .3f) % 2 == 0 ? "#E0B652" : "#252C38", -4);
        }
        view.safeMarks = new GameObject("Sem corrente — marcas verdes");
        view.warningMarks = new GameObject("Carregando — exclamações");
        view.electricity = new GameObject("Corrente — descargas");
        foreach (var group in new[] { view.safeMarks, view.warningMarks, view.electricity })
            group.transform.SetParent(root, false);
        var lamps = new List<SpriteRenderer>();
        for (float x = -size.x / 2 + .55f; x < size.x / 2 - .25f; x += 1.15f)
        {
            Vector2 p = center + new Vector2(x, 0);
            Segment("Livre", view.safeMarks.transform, p + new Vector2(-.15f,0), p + new Vector2(-.03f,-.12f), .075f, "#83F4BA");
            Segment("Livre", view.safeMarks.transform, p + new Vector2(-.03f,-.12f), p + new Vector2(.19f,.16f), .075f, "#83F4BA");
            Solid("Alerta", view.warningMarks.transform, p + Vector2.up * .08f, new Vector2(.1f,.25f), "#FFE29B", -3);
            Solid("Alerta", view.warningMarks.transform, p - Vector2.up * .16f, new Vector2(.1f,.1f), "#FFE29B", -3);
            Segment("Arco", view.electricity.transform, p + new Vector2(.14f,.27f), p + new Vector2(-.13f,.01f), .08f, "#E7FCFF");
            Segment("Arco", view.electricity.transform, p + new Vector2(-.13f,.01f), p + new Vector2(.13f,-.01f), .08f, "#E7FCFF");
            Segment("Arco", view.electricity.transform, p + new Vector2(.13f,-.01f), p + new Vector2(-.14f,-.27f), .08f, "#E7FCFF");
            var lamp = Draw("Luz de estado", root, square, p + Vector2.up * (size.y / 2 - .22f), new Vector2(.17f,.08f), -3);
            lamps.Add(lamp);
        }
        view.lamps = lamps.ToArray();
        view.arcs = view.electricity.GetComponentsInChildren<SpriteRenderer>();
    }

    private static void Segment(string name, Transform parent, Vector2 a, Vector2 b, float width, string color)
    {
        var line = Draw(name, parent, square, (a+b)/2, new Vector2((b-a).magnitude,width), -3);
        line.transform.rotation = Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);
        ColorUtility.TryParseHtmlString(color, out var tint); line.color = tint;
    }

    private static void Border(string name, Transform root, Vector2 center, Vector2 size, string color, int order)
    {
        const float width = .05f;
        Solid(name + " superior", root, center + Vector2.up * (size.y - width) / 2, new Vector2(size.x, width), color, order);
        Solid(name + " inferior", root, center - Vector2.up * (size.y - width) / 2, new Vector2(size.x, width), color, order);
        Solid(name + " esquerda", root, center - Vector2.right * (size.x - width) / 2, new Vector2(width, size.y), color, order);
        Solid(name + " direita", root, center + Vector2.right * (size.x - width) / 2, new Vector2(width, size.y), color, order);
    }

    private static Vector2 WorldSize(BoxCollider2D collider)
    {
        return Vector2.Scale(collider.size, collider.transform.lossyScale);
    }

    private static Sprite[] CharacterFrames(string name)
    {
        var atlas = Texture(Generated + name + ".png", 128);
        var columns = OccupiedBands(atlas, true);
        var rows = OccupiedBands(atlas, false);
        if (columns.Count != 4 || rows.Count != 4)
            throw new InvalidOperationException(name + ".png precisa conter quatro linhas e quatro colunas separadas por transparência.");
        var frames = new Sprite[16];
        for (int row = 0; row < 4; row++)
        for (int column = 0; column < 4; column++)
        {
            Rect region = new Rect(columns[column].x, rows[row].x, columns[column].y, rows[row].y);
            frames[row * 4 + column] = Slice(atlas, name + "_" + row + "_" + column, AlphaBounds(atlas, region), 24,
                new Vector2(.5f, .28f));
        }
        return frames;
    }

    private static PrisonCharacterVisual DressCharacter(GameController game, Transform body, SpriteRenderer prototype,
        Sprite[] frames, string name)
    {
        var visual = new GameObject(name, typeof(SpriteRenderer));
        visual.transform.SetParent(body, false);
        visual.transform.localScale = new Vector3(1 / body.lossyScale.x, 1 / body.lossyScale.y, 1);
        visual.transform.localPosition = new Vector3(0, 0, -.02f);
        var renderer = visual.GetComponent<SpriteRenderer>();
        renderer.sprite = frames[0]; renderer.sortingOrder = 5; renderer.sharedMaterial = material;
        prototype.enabled = false;
        var animation = body.gameObject.AddComponent<PrisonCharacterVisual>();
        animation.game = game; animation.prototype = prototype;
        animation.character = renderer; animation.frames = frames;
        return animation;
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

    private static SpriteRenderer Tiled(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size, int order)
    {
        var renderer = Draw(name, parent, sprite, position, sprite.bounds.size, order);
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = size;
        return renderer;
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
