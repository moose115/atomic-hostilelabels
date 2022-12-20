using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AtomicHostileLabels
{
  /// <summary>
  /// Copy of vanilla GenMapUI. Used to ensure label rendering on hostile animals and mechanoids with mods like Camera+.
  /// </summary>
  [StaticConstructorOnStartup]
  public static class GenMapUI2
  {
    public static readonly Texture2D OverlayHealthTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.0f, 0.0f, 0.25f));
    public static readonly Texture2D OverlayEntropyTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.55f, 0.84f, 0.5f));
    public const float NameBGHeight_Tiny = 12f;
    public const float NameBGExtraWidth_Tiny = 4f;
    public const float NameBGHeight_Small = 16f;
    public const float NameBGExtraWidth_Small = 6f;
    public const float LabelOffsetYStandard = -0.4f;
    public const float PsychicEntropyBarHeight = 4f;
    private const float AnimalLabelNudgeUpPixels = 4f;
    private const float BabyLabelNudgeUpPixels = 8f;
    public static readonly Color DefaultThingLabelColor = new Color(1f, 1f, 1f, 0.75f);

    public static Vector2 LabelDrawPosFor(Thing thing, float worldOffsetZ)
    {
      Vector3 drawPos = thing.DrawPos;
      drawPos.z += worldOffsetZ;
      Vector2 vector2 = (Vector2) (Find.Camera.WorldToScreenPoint(drawPos) / Prefs.UIScale);
      vector2.y = (float) UI.screenHeight - vector2.y;
      if (thing is Pawn)
      {
        Pawn pawn = (Pawn) thing;
        if (!pawn.RaceProps.Humanlike)
          vector2.y -= 4f;
        else if (pawn.DevelopmentalStage.Baby())
          vector2.y -= 8f;
      }
      return vector2;
    }

    public static Vector2 LabelDrawPosFor(IntVec3 center)
    {
      Vector2 vector2 = (Vector2) (Find.Camera.WorldToScreenPoint(center.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays)) / Prefs.UIScale);
      vector2.y = (float) UI.screenHeight - vector2.y;
      --vector2.y;
      return vector2;
    }

    public static void DrawThingLabel(Thing thing, string text) => GenMapUI2.DrawThingLabel(thing, text, GenMapUI2.DefaultThingLabelColor);

    public static void DrawThingLabel(Thing thing, string text, Color textColor) => GenMapUI2.DrawThingLabel(GenMapUI2.LabelDrawPosFor(thing, -0.4f), text, textColor);

    public static void DrawThingLabel(Vector2 screenPos, string text, Color textColor)
    {
      Text.Font = GameFont.Tiny;
      float x = Text.CalcSize(text).x;
      float num = Text.TinyFontSupported ? 4f : 6f;
      float height = Text.TinyFontSupported ? 12f : 16f;
      GUI.DrawTexture(new Rect(screenPos.x - x / 2f - num, screenPos.y, x + num * 2f, height), (Texture) TexUI.GrayTextBG);
      GUI.color = textColor;
      Text.Anchor = TextAnchor.UpperCenter;
      Widgets.Label(new Rect(screenPos.x - x / 2f, screenPos.y - 3f, x, 999f), text);
      GUI.color = Color.white;
      Text.Anchor = TextAnchor.UpperLeft;
      Text.Font = GameFont.Small;
    }

    public static void DrawPawnLabel(
      Pawn pawn,
      Vector2 pos,
      float alpha = 1f,
      float truncateToWidth = 9999f,
      Dictionary<string, string> truncatedLabelsCache = null,
      GameFont font = GameFont.Tiny,
      bool alwaysDrawBg = true,
      bool alignCenter = true)
    {
      float pawnLabelNameWidth = GenMapUI2.GetPawnLabelNameWidth(pawn, truncateToWidth, truncatedLabelsCache, font);
      float num = Prefs.DisableTinyText ? 6f : 4f;
      float height = Prefs.DisableTinyText ? 16f : 12f;
      Rect bgRect = new Rect(pos.x - pawnLabelNameWidth / 2f - num, pos.y, pawnLabelNameWidth + num * 2f, height);
      GenMapUI2.DrawPawnLabel(pawn, bgRect, alpha, truncateToWidth, truncatedLabelsCache, font, alwaysDrawBg, alignCenter);
    }

    public static void DrawPawnLabel(
      Pawn pawn,
      Rect bgRect,
      float alpha = 1f,
      float truncateToWidth = 9999f,
      Dictionary<string, string> truncatedLabelsCache = null,
      GameFont font = GameFont.Tiny,
      bool alwaysDrawBg = true,
      bool alignCenter = true)
    {
      GUI.color = new Color(1f, 1f, 1f, alpha);
      Text.Font = font;
      string pawnLabel = GenMapUI2.GetPawnLabel(pawn, truncateToWidth, truncatedLabelsCache, font);
      float pawnLabelNameWidth = GenMapUI2.GetPawnLabelNameWidth(pawn, truncateToWidth, truncatedLabelsCache, font);
      float summaryHealthPercent = pawn.health.summaryHealth.SummaryHealthPercent;
      if (alwaysDrawBg || (double) summaryHealthPercent < 0.9990000128746033)
        GUI.DrawTexture(bgRect, (Texture) TexUI.GrayTextBG);
      if ((double) summaryHealthPercent < 0.9990000128746033)
        Widgets.FillableBar(bgRect.ContractedBy(1f), summaryHealthPercent, GenMapUI2.OverlayHealthTex, BaseContent.ClearTex, false);
      GUI.color = PawnNameColorUtility.PawnNameColorOf(pawn) with
      {
        a = alpha
      };
      Rect rect;
      if (alignCenter)
      {
        Text.Anchor = TextAnchor.UpperCenter;
        rect = new Rect(bgRect.center.x - pawnLabelNameWidth / 2f, bgRect.y - 2f, pawnLabelNameWidth, 100f);
      }
      else
      {
        Text.Anchor = TextAnchor.UpperLeft;
        rect = new Rect(bgRect.x + 2f, bgRect.center.y - Text.CalcSize(pawnLabel).y / 2f, pawnLabelNameWidth, 100f);
      }
      Widgets.Label(rect, pawnLabel);
      if (pawn.Drafted)
        Widgets.DrawLineHorizontal(bgRect.center.x - pawnLabelNameWidth / 2f, (float) ((double) bgRect.y + 11.0 + (Text.TinyFontSupported ? 0.0 : 3.0)), pawnLabelNameWidth);
      GUI.color = Color.white;
      Text.Anchor = TextAnchor.UpperLeft;
    }
    
    public static void DrawText(Vector2 worldPos, string text, Color textColor)
    {
      Vector2 vector2 = (Vector2) (Find.Camera.WorldToScreenPoint(new Vector3(worldPos.x, 0.0f, worldPos.y)) / Prefs.UIScale);
      vector2.y = (float) UI.screenHeight - vector2.y;
      Text.Font = GameFont.Tiny;
      GUI.color = textColor;
      Text.Anchor = TextAnchor.UpperCenter;
      float x = Text.CalcSize(text).x;
      Widgets.Label(new Rect(vector2.x - x / 2f, vector2.y - 2f, x, 999f), text);
      GUI.color = Color.white;
      Text.Anchor = TextAnchor.UpperLeft;
    }

    private static float GetPawnLabelNameWidth(
      Pawn pawn,
      float truncateToWidth,
      Dictionary<string, string> truncatedLabelsCache,
      GameFont font)
    {
      int font1 = (int) Text.Font;
      Text.Font = font;
      string pawnLabel = GenMapUI2.GetPawnLabel(pawn, truncateToWidth, truncatedLabelsCache, font);
      float pawnLabelNameWidth = font != GameFont.Tiny ? Text.CalcSize(pawnLabel).x : pawnLabel.GetWidthCached();
      if (Math.Abs(Math.Round((double) Prefs.UIScale) - (double) Prefs.UIScale) > 1.401298464324817E-45)
        pawnLabelNameWidth += 0.5f;
      if ((double) pawnLabelNameWidth < 20.0)
        pawnLabelNameWidth = 20f;
      Text.Font = (GameFont) font1;
      return pawnLabelNameWidth;
    }

    private static string GetPawnLabel(
      Pawn pawn,
      float truncateToWidth,
      Dictionary<string, string> truncatedLabelsCache,
      GameFont font)
    {
      GameFont font1 = Text.Font;
      Text.Font = font;
      string pawnLabel = pawn.LabelShortCap.Truncate(truncateToWidth, truncatedLabelsCache);
      Text.Font = font1;
      return pawnLabel;
    }
  }
}