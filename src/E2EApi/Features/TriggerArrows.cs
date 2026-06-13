using System.Collections.Generic;
using E2EApi.Editor;
using UnityEngine;

namespace E2EApi.Features
{
    /// <summary>
    /// Logic-connection visualizer: draws an arrow from every trigger button to
    /// its linked target. Clicking an arrow jumps the editor camera to the
    /// linked end; clicking it again jumps back (alternating).
    /// </summary>
    public static class TriggerArrows
    {
        public static bool Enabled;

        private const float ClickRadiusPx = 14f;

        private class Arrow
        {
            public Triggers.Link Link;
            public GameObject Line;
            public GameObject Head;
            public bool JumpToSource; // flips on every click
        }

        private static readonly List<Arrow> Arrows = new List<Arrow>();
        private static int _builtVersion = -1;

        internal static void Tick()
        {
            if (!Enabled || !Events.GameEvents.IsEditorActive)
            {
                if (Arrows.Count > 0)
                {
                    DestroyArrows();
                }
                return;
            }
            if (_builtVersion != Triggers.Version)
            {
                Rebuild();
                _builtVersion = Triggers.Version;
            }
            HandleClick();
        }

        private static void Rebuild()
        {
            DestroyArrows();
            float scale = FenceOverlay.TileWorldSize();
            foreach (var link in Triggers.All)
            {
                Vector3? from = Grid.TileToWorld(link.SourceX, link.SourceY);
                Vector3? to = Grid.TileToWorld(link.TargetX, link.TargetY);
                if (from == null || to == null)
                {
                    continue;
                }
                Vector3 a = new Vector3(from.Value.x, from.Value.y, from.Value.z - 0.65f);
                Vector3 b = new Vector3(to.Value.x, to.Value.y, to.Value.z - 0.65f);

                var lineGo = new GameObject("E2E_TriggerArrow");
                var line = lineGo.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.sharedMaterial = OverlayLib.SpriteMaterial;
                line.SetColors(new Color(1f, 0.5f, 0.1f, 0.9f), new Color(1f, 0.9f, 0.2f, 0.9f));
                line.SetWidth(scale * 0.12f, scale * 0.04f);
                line.SetVertexCount(2);
                line.SetPosition(0, a);
                line.SetPosition(1, b);

                // arrowhead: small quad rotated towards the target
                var head = OverlayLib.MakeTileQuad("E2E_TriggerArrowHead",
                    OverlayLib.OutlineTexture, link.TargetX, link.TargetY, -0.66f);
                if (head != null)
                {
                    head.transform.localScale = new Vector3(scale * 0.4f, scale * 0.4f, 1f);
                    Vector3 dir = b - a;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    head.transform.rotation = Quaternion.Euler(0f, 0f, angle + 45f);
                    var renderer = head.GetComponent<SpriteRenderer>();
                    renderer.color = new Color(1f, 0.8f, 0.2f, 0.95f);
                }

                Arrows.Add(new Arrow { Link = link, Line = lineGo, Head = head });
            }
        }

        private static void HandleClick()
        {
            if (!Input.GetMouseButtonDown(0) || Arrows.Count == 0)
            {
                return;
            }
            var controller = LevelEditor_Controller.GetInstance();
            var camera = controller != null ? controller.m_MainCamera : null;
            if (camera == null)
            {
                return;
            }
            Vector2 mouse = Input.mousePosition;

            Arrow best = null;
            float bestDistance = ClickRadiusPx;
            foreach (var arrow in Arrows)
            {
                var line = arrow.Line != null ? arrow.Line.GetComponent<LineRenderer>() : null;
                if (line == null)
                {
                    continue;
                }
                Vector3? from = Grid.TileToWorld(arrow.Link.SourceX, arrow.Link.SourceY);
                Vector3? to = Grid.TileToWorld(arrow.Link.TargetX, arrow.Link.TargetY);
                if (from == null || to == null)
                {
                    continue;
                }
                Vector2 a = camera.WorldToScreenPoint(from.Value);
                Vector2 b = camera.WorldToScreenPoint(to.Value);
                float distance = OverlayLib.PointToSegment(mouse, a, b);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = arrow;
                }
            }
            if (best == null)
            {
                return;
            }
            if (best.JumpToSource)
            {
                EditorCamera.JumpTo(best.Link.SourceX, best.Link.SourceY);
            }
            else
            {
                EditorCamera.JumpTo(best.Link.TargetX, best.Link.TargetY);
            }
            best.JumpToSource = !best.JumpToSource;
        }

        private static void DestroyArrows()
        {
            foreach (var arrow in Arrows)
            {
                if (arrow.Line != null)
                {
                    Object.Destroy(arrow.Line);
                }
                if (arrow.Head != null)
                {
                    Object.Destroy(arrow.Head);
                }
            }
            Arrows.Clear();
            _builtVersion = -1;
        }
    }
}
