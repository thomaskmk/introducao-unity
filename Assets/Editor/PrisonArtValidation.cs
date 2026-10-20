using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ObstacleCourse;

// Compare the saved, approved course with its rebuilt visual version.
// This command deliberately rebuilds the scene, just like CourseBuilder.Generate.
public static class PrisonArtValidation
{
    public static void RebuildPreservingGameplay()
    {
        EditorSceneManager.OpenScene(CourseBuilder.ScenePath);
        var before = Capture();
        CourseBuilder.Generate();
        var after = Capture();
        Directory.CreateDirectory("Logs");
        File.WriteAllLines("Logs/gameplay-before.txt", before);
        File.WriteAllLines("Logs/gameplay-after.txt", after);
        if (!before.SequenceEqual(after))
            throw new InvalidOperationException("A geometria ou configuração de jogo mudou. Compare Logs/gameplay-before.txt e gameplay-after.txt.");
        Debug.Log("ARTE VALIDADA: " + after.Length + " registros de física e regras idênticos à cena anterior.");
    }

    private static string[] Capture()
    {
        var game = UnityEngine.Object.FindFirstObjectByType<GameController>();
        var rows = new List<string>();
        var roots = game.levels.Select(l => l.transform).Concat(new[] { game.player.transform });
        foreach (var root in roots)
        {
            foreach (var collider in root.GetComponentsInChildren<Collider2D>(true))
            {
                if (!(collider is BoxCollider2D box))
                    throw new InvalidOperationException("Adicione a captura do novo tipo de collider: " + collider.GetType());
                rows.Add("COLLIDER|" + Path(box.transform) + "|" + Pose(box.transform) +
                    "|" + box.enabled + "|" + box.isTrigger + "|" + Value(box.offset) + "|" + Value(box.size) +
                    "|" + Value(box.edgeRadius) + "|" + Value(box.density) + "|" + box.autoTiling +
                    "|" + AssetDatabase.GetAssetPath(box.sharedMaterial));
            }
            foreach (var body in root.GetComponentsInChildren<Rigidbody2D>(true))
                rows.Add("BODY|" + Path(body.transform) + "|" + Pose(body.transform) +
                    "|" + body.bodyType + "|" + body.simulated + "|" + Value(body.mass) + "|" + body.useAutoMass +
                    "|" + Value(body.gravityScale) + "|" + Value(body.linearDamping) + "|" + Value(body.angularDamping) +
                    "|" + body.constraints + "|" + body.collisionDetectionMode + "|" + body.interpolation + "|" + body.sleepMode);
            foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (!(component is CourseLevel || component is PlayerController2D || component is ChaserEnemy ||
                    component is PatrolHazard || component is RotatingHazard || component is CycleTrap ||
                    component is SlowZone || component is FinishZone)) continue;
                var values = new List<string>();
                foreach (var field in component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType)) continue;
                    if (field.FieldType.IsArray)
                    {
                        // Array membership/count matters; scene-local object IDs change on reconstruction.
                        var array = (Array)field.GetValue(component);
                        values.Add(field.Name + "=" + string.Join(",", array.Cast<UnityEngine.Object>()
                            .Select(o => o is Component c ? Path(c.transform) : o.name)));
                    }
                    else values.Add(field.Name + "=" + Value(field.GetValue(component)));
                }
                values.Sort(StringComparer.Ordinal);
                rows.Add("RULE|" + component.GetType().Name + "|" + Path(component.transform) +
                    "|" + component.enabled + "|" + string.Join("|", values));
            }
        }
        rows.Sort(StringComparer.Ordinal);
        return rows.ToArray();
    }

    private static string Pose(Transform transform)
    {
        return Value(transform.position) + "|" + Value(transform.rotation) + "|" + Value(transform.lossyScale) +
            "|" + transform.gameObject.layer + "|" + transform.gameObject.tag + "|" + transform.gameObject.activeInHierarchy;
    }

    private static string Value(object value)
    {
        if (value is string text) return text;
        if (value is Vector2 || value is Vector3 || value is Quaternion || value is Rect || value is LayerMask)
            return JsonUtility.ToJson(value);
        if (value is float number) return number.ToString("R", CultureInfo.InvariantCulture);
        if (value is IFormattable formatted) return formatted.ToString(null, CultureInfo.InvariantCulture);
        if (value is bool flag) return flag.ToString();
        return JsonUtility.ToJson(value);
    }

    private static string Path(Transform transform)
    {
        return transform.parent == null ? transform.name : Path(transform.parent) + "/" + transform.name;
    }
}
