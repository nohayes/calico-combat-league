using System.Collections.Generic;
using UnityEngine;

public enum IconShape
{
    Circle,
    Triangle,
    Diamond,
    DoubleRing,
    Star
}

// Generates small, simple placeholder sprites (discipline icons, a fighter
// silhouette) entirely in code so no external art is required yet. Sprites
// are generated once and cached - see ArtRegistry for where real art would
// eventually be loaded from instead.
public static class IconFactory
{
    static readonly Dictionary<IconShape, Sprite> iconCache = new Dictionary<IconShape, Sprite>();

    static Sprite silhouetteSprite;

    public static Color GetGymThemeColor(GymType type)
    {
        switch (type)
        {
            case GymType.Boxing: return new Color(0.82f, 0.34f, 0.13f, 1f);
            case GymType.MuayThai: return new Color(0.80f, 0.58f, 0.16f, 1f);
            case GymType.Wrestling: return new Color(0.36f, 0.55f, 0.30f, 1f);
            case GymType.BrazilianJiuJitsu: return new Color(0.33f, 0.42f, 0.66f, 1f);
            case GymType.Championship: return UIFactory.GoldColor;
            default: return UIFactory.AccentOrange;
        }
    }

    public static IconShape GetGymIconShape(GymType type)
    {
        switch (type)
        {
            case GymType.Boxing: return IconShape.Circle;
            case GymType.MuayThai: return IconShape.Triangle;
            case GymType.Wrestling: return IconShape.DoubleRing;
            case GymType.BrazilianJiuJitsu: return IconShape.Diamond;
            case GymType.Championship: return IconShape.Star;
            default: return IconShape.Circle;
        }
    }

    // Archetype accents intentionally mirror the matching gym's icon/color, so a
    // Boxer's portrait badge looks like the Boxing Gym's icon, and so on - one
    // consistent visual language for "what discipline is this fighter."
    public static IconShape GetArchetypeIconShape(ArchetypeType type)
    {
        switch (type)
        {
            case ArchetypeType.Boxer: return IconShape.Circle;
            case ArchetypeType.Wrestler: return IconShape.DoubleRing;
            case ArchetypeType.BjjSpecialist: return IconShape.Diamond;
            case ArchetypeType.MuayThaiFighter: return IconShape.Triangle;
            default: return IconShape.Circle;
        }
    }

    public static Color GetArchetypeThemeColor(ArchetypeType type)
    {
        switch (type)
        {
            case ArchetypeType.Boxer: return GetGymThemeColor(GymType.Boxing);
            case ArchetypeType.Wrestler: return GetGymThemeColor(GymType.Wrestling);
            case ArchetypeType.BjjSpecialist: return GetGymThemeColor(GymType.BrazilianJiuJitsu);
            case ArchetypeType.MuayThaiFighter: return GetGymThemeColor(GymType.MuayThai);
            default: return UIFactory.AccentOrange;
        }
    }

    // Move-type icons reuse the same shape language as their parent discipline,
    // with sensible fallbacks for move types that don't have their own gym yet.
    public static IconShape GetMoveTypeIconShape(MoveType type)
    {
        switch (type)
        {
            case MoveType.Boxing: return IconShape.Circle;
            case MoveType.Kickboxing:
            case MoveType.MuayThai:
            case MoveType.Karate:
            case MoveType.Taekwondo: return IconShape.Triangle;
            case MoveType.Wrestling:
            case MoveType.Judo: return IconShape.DoubleRing;
            case MoveType.BrazilianJiuJitsu: return IconShape.Diamond;
            case MoveType.GroundAndPound: return IconShape.Star;
            default: return IconShape.Circle;
        }
    }

    public static IconShape GetItemIconShape(ItemEffectType type)
    {
        switch (type)
        {
            case ItemEffectType.RestoreStamina: return IconShape.Triangle;
            case ItemEffectType.RestoreHealth: return IconShape.Circle;
            case ItemEffectType.CombatBuff: return IconShape.Diamond;
            default: return IconShape.Circle;
        }
    }

    public static IconShape GetAchievementIconShape(AchievementMetric metric)
    {
        switch (metric)
        {
            case AchievementMetric.TotalWins: return IconShape.Circle;
            case AchievementMetric.GymsCleared: return IconShape.DoubleRing;
            case AchievementMetric.MaxSingleHitDamage: return IconShape.Triangle;
            case AchievementMetric.SubmissionWins: return IconShape.Diamond;
            case AchievementMetric.BecameChampion: return IconShape.Star;
            case AchievementMetric.CoinsSpent: return IconShape.Circle;
            case AchievementMetric.MovesKnown: return IconShape.Diamond;
            default: return IconShape.Circle;
        }
    }

    public static Sprite GetShapeSprite(IconShape shape)
    {
        if (iconCache.TryGetValue(shape, out var cached)) return cached;

        var sprite = GenerateShapeSprite(shape);
        iconCache[shape] = sprite;
        return sprite;
    }

    public static Sprite GetSilhouetteSprite()
    {
        return silhouetteSprite ??= GenerateSilhouetteSprite();
    }

    static Sprite GenerateShapeSprite(IconShape shape)
    {
        const int size = 96;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float alpha = ShapeAlpha(shape, x, y, size);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    static float ShapeAlpha(IconShape shape, int x, int y, int size)
    {
        float cx = size / 2f;
        float cy = size / 2f;
        float px = x + 0.5f - cx;
        float py = y + 0.5f - cy;
        float r = size * 0.36f;

        switch (shape)
        {
            case IconShape.Circle:
                return (px * px + py * py <= r * r) ? 1f : 0f;

            case IconShape.Diamond:
                return (Mathf.Abs(px) + Mathf.Abs(py) <= r) ? 1f : 0f;

            case IconShape.Triangle:
            {
                var a = new Vector2(0f, -r);
                var b = new Vector2(-r * 0.87f, r * 0.6f);
                var c = new Vector2(r * 0.87f, r * 0.6f);
                return PointInTriangle(new Vector2(px, py), a, b, c) ? 1f : 0f;
            }

            case IconShape.DoubleRing:
            {
                float ringR = size * 0.24f;
                float ringThickness = ringR * 0.45f;
                float offset = size * 0.15f;
                float d1 = Mathf.Sqrt((px - offset) * (px - offset) + py * py);
                float d2 = Mathf.Sqrt((px + offset) * (px + offset) + py * py);
                bool inRing1 = d1 <= ringR && d1 >= ringR - ringThickness;
                bool inRing2 = d2 <= ringR && d2 >= ringR - ringThickness;
                return (inRing1 || inRing2) ? 1f : 0f;
            }

            case IconShape.Star:
            {
                float dist = Mathf.Sqrt(px * px + py * py);
                float angle = Mathf.Atan2(py, px) + Mathf.PI / 2f;
                float points = 5f;
                float wave = Mathf.Abs(Mathf.Cos(points * angle * 0.5f));
                float radiusAtAngle = Mathf.Lerp(r * 0.45f, r, wave);
                return dist <= radiusAtAngle ? 1f : 0f;
            }

            default:
                return 0f;
        }
    }

    static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Cross(b - a, p - a);
        float d2 = Cross(c - b, p - b);
        float d3 = Cross(a - c, p - c);

        bool hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
        bool hasPos = d1 > 0 || d2 > 0 || d3 > 0;
        return !(hasNeg && hasPos);
    }

    static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    static Sprite GenerateSilhouetteSprite()
    {
        const int size = 96;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };

        float headRadius = size * 0.16f;
        float headCenterY = size * 0.72f;
        float shoulderHalfWidth = size * 0.28f;
        float hipHalfWidth = size * 0.20f;
        float bodyTop = size * 0.56f;
        float bodyBottom = size * 0.05f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f - size / 2f;
                float py = y + 0.5f;

                float headDy = py - headCenterY;
                bool inHead = (px * px + headDy * headDy) <= headRadius * headRadius;

                bool inBody = false;
                if (py >= bodyBottom && py <= bodyTop)
                {
                    float t = (py - bodyBottom) / (bodyTop - bodyBottom);
                    float halfWidth = Mathf.Lerp(hipHalfWidth, shoulderHalfWidth, t);
                    inBody = Mathf.Abs(px) <= halfWidth;
                }

                float alpha = (inHead || inBody) ? 1f : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
