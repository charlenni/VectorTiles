﻿using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VectorTiles.Enums;
using VectorTiles.MapboxGL.Expressions;

using Rect = SkiaSharp.SKRect;

namespace VectorTiles.MapboxGL
{
    public class MGLSymbolStyler : IVectorSymbolStyler
    {
        static Regex regex = new Regex(@".*\{(.*)\}.*");

        MGLSpriteAtlas spriteAtlas;

        public static MGLSymbolStyler Default;

        public MGLSymbolStyler(MGLSpriteAtlas atlas)
        {
            spriteAtlas = atlas;
        }

        public bool HasIcon { get => IconImage != null; }

        public bool HasText { get => TextField != null && TextFont != null; }

        public bool IconAllowOverlap { get; internal set; }

        public Direction IconAnchor { get; internal set; }

        public StoppedColor IconColor { get; internal set; } = new StoppedColor() { SingleVal = new SKColor(0, 0, 0, 0) };

        public StoppedFloat IconHaloBlur { get; internal set; } = new StoppedFloat() { SingleVal = 0 };

        public StoppedColor IconHaloColor { get; internal set; } = new StoppedColor() { SingleVal = new SKColor(0, 0, 0, 0) };

        public StoppedFloat IconHaloWidth { get; internal set; } = new StoppedFloat() { SingleVal = 0 };

        public bool IconIgnorePlacement { get; internal set; }

        public StoppedString IconImage { get; internal set; }

        public bool IconKeepUpright { get; internal set; }

        public Offset IconOffset { get; internal set; } = Offset.Empty;

        public StoppedFloat IconOpacity { get; internal set; } = new StoppedFloat() { SingleVal = 1 };

        public bool IconOptional { get; internal set; }

        public StoppedFloat IconPadding { get; internal set; } = new StoppedFloat() { SingleVal = 2 };

        public MapAlignment IconPitchAlignment { get; internal set; } = MapAlignment.Auto;

        public StoppedFloat IconRotate { get; internal set; } = new StoppedFloat() { SingleVal = 0 };

        public MapAlignment IconRotationAlignment { get; internal set; } = MapAlignment.Auto;

        public StoppedFloat IconSize { get; internal set; } = new StoppedFloat() { SingleVal = 1 };

        public TextFit IconTextFit { get; internal set; } = TextFit.None;

        public Rect IconTextFitPadding { get; internal set; } = Rect.Empty;

        public Offset IconTranslate { get; internal set; } = Offset.Empty;

        public MapAlignment IconTranslateAnchor { get; internal set; } = MapAlignment.Map;

        public bool SymbolAvoidEdges { get; internal set; }

        public StoppedString SymbolPlacement { get; internal set; } = new StoppedString() { SingleVal = "point" };

        public float SymbolSortKey { get; internal set; }

        public StoppedFloat SymbolSpacing { get; internal set; } = new StoppedFloat() { SingleVal = 250 };

        public ZOrder SymbolZOrder { get; internal set; } = ZOrder.Auto;

        public bool TextAllowOverlap { get; internal set; }

        public Direction TextAnchor { get; internal set; } = Direction.Center;

        public StoppedColor TextColor { get; internal set; } = new StoppedColor() { SingleVal = new SKColor(0, 0, 0, 0) };

        public string TextField { get; internal set; } = "";

        public List<string> TextFont { get; internal set; } = new List<string>();

        public StoppedFloat TextHaloBlur { get; internal set; } = new StoppedFloat() { SingleVal = 0 };

        public StoppedColor TextHaloColor { get; internal set; } = new StoppedColor() { SingleVal = new SKColor(0, 0, 0, 0) };

        public StoppedFloat TextHaloWidth { get; internal set; } = new StoppedFloat() { SingleVal = 0 };

        public bool TextIgnorePlacement { get; internal set; }

        public TextJustify TextJustify { get; internal set; } = TextJustify.Center;

        public bool TextKeepUpright { get; internal set; }

        public StoppedFloat TextLetterSpacing { get; internal set; } = new StoppedFloat() { SingleVal = 0 };

        public StoppedFloat TextLineHeight { get; internal set; } = new StoppedFloat() { SingleVal = 1.2f };

        public StoppedFloat TextMaxAngle { get; internal set; } = new StoppedFloat() { SingleVal = 45 };

        public StoppedFloat TextMaxWidth { get; internal set; } = new StoppedFloat() { SingleVal = 10 };

        public Offset TextOffset { get; internal set; } = Offset.Empty;

        public StoppedFloat TextOpacity { get; internal set; } = new StoppedFloat() { SingleVal = 1 };

        public bool TextOptional { get; internal set; }

        public StoppedFloat TextPadding { get; internal set; } = new StoppedFloat() { SingleVal = 2 };

        public MapAlignment TextPitchAlignment { get; internal set; } = MapAlignment.Auto;

        public StoppedFloat TextRadialOffset { get; internal set; } = new StoppedFloat() { SingleVal = 0 };

        public StoppedFloat TextRotate { get; internal set; } = new StoppedFloat() { SingleVal = 0 };

        public MapAlignment TextRotationAlignment { get; internal set; } = MapAlignment.Auto;

        public StoppedFloat TextSize { get; internal set; } = new StoppedFloat() { SingleVal = 16 };

        public TextTransform TextTransform { get; internal set; } = TextTransform.None;

        public Offset TextTranslate { get; internal set; } = Offset.Empty;

        public MapAlignment TextTranslateAnchor { get; internal set; } = MapAlignment.Map;

        public List<MapAlignment> TextVariableAnchor { get; internal set; } = new List<MapAlignment>();

        public List<Orientation> TextWritingMode { get; internal set; } = new List<Orientation>();

        public Symbol CreateIconSymbol(SKPoint point, TagsCollection tags, EvaluationContext context)
        {
            if (IconImage == null)
                return null;

            var result = new MGLIconSymbol();

            var iconName = ReplaceWithTags(IconImage.Evaluate(context.Zoom), tags, context);

            if (!string.IsNullOrEmpty(iconName))
            {
                result.Image = spriteAtlas.GetSprite(iconName).Image;
                result.ImagePoint = point;
                result.Paint = new MGLPaint();
            }

            result.OnCalcBoundings();

            return result;
        }

        public Symbol CreateTextSymbol(SKPoint point, TagsCollection tags, EvaluationContext context)
        {
            if (TextField == null)
                return null;

            var result = new MGLTextSymbol();

            var fieldName = ReplaceWithTags(TextField, tags, context);

            return result;
        }

        public Symbol CreateIconTextSymbol(SKPoint point, TagsCollection tags, EvaluationContext context)
        {
            if (IconImage == null && TextField == null)
                return null;

            var result = new MGLIconTextSymbol();

            var iconName = ReplaceWithTags(IconImage.Evaluate(context.Zoom), tags, context);
            var fieldName = ReplaceWithTags(TextField, tags, context);

            if (!string.IsNullOrEmpty(iconName))
            {
                result.Image = spriteAtlas.GetSprite(iconName)?.Image;
                result.ImagePoint = point;
                result.Paint = new MGLPaint();
            }

            return result;
        }

        public IEnumerable<Symbol> CreatePathSymbols(VectorElement element, EvaluationContext context)
        {
            return null;
        }

        private string ReplaceWithTags(string text, TagsCollection tags, EvaluationContext context)
        {
            var match = regex.Match(text);

            if (!match.Success)
                return text;

            var val = match.Groups[1].Value;

            if (tags.ContainsKey(val))
                return text.Replace($"{{{val}}}", (string)tags[val]);

            if (context.Tags != null && context.Tags.ContainsKey(val))
                return text.Replace($"{{{val}}}", (string)context.Tags[val]);

            // Check, if match starts with name
            if (val.StartsWith("name"))
            {
                // TODO: Try to take the localized name    
            }

            return text;
        }
    }
}
