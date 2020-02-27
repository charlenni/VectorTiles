using SkiaSharp;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VectorTiles.MapboxGL.Json;

namespace VectorTiles.MapboxGL.Converter
{
    public static class StyleLayerConverter
    {
        /// <summary>
        /// Convert given context with Mapbox GL styling layer to a Mapsui Style list
        /// </summary>
        /// <param name="context">Context to use while evaluating style</param>
        /// <param name="styleLayer">Mapbox GL style layer</param>
        /// <param name="spriteAtlas">Dictionary with availible sprites</param>
        /// <returns>A list of Mapsui Styles</returns>
        public static List<MGLPaint> Convert(EvaluationContext context, StyleLayer styleLayer, MGLSpriteAtlas spriteAtlas)
        {
            switch (styleLayer.Type)
            {
                case "fill":
                    return ConvertFillLayer(styleLayer, spriteAtlas);
                case "line":
                    return ConvertLineLayer(styleLayer, spriteAtlas);
                case "symbol":
                    return ConvertSymbolLayer(styleLayer, spriteAtlas);
                case "circle":
                    return new List<MGLPaint>();
                case "raster":
                    // Shouldn't get here, because raster are directly handled by ConvertRasterLayer
                    break;
                case "background":
                    return ConvertBackgroundLayer(styleLayer, spriteAtlas);
            }

            return new List<MGLPaint>();
        }

        public static List<MGLPaint> ConvertBackgroundLayer(StyleLayer style, MGLSpriteAtlas spriteAtlas)
        {
            // visibility
            //   Optional enum. One of visible, none. Defaults to visible.
            //   The display of this layer. none hides this layer.
            if (style.Layout?.Visibility != null && style.Layout.Visibility.Equals("none"))
                return null;

            var paint = style.Paint;

            var brush = new MGLPaint();

            brush.SetFixColor(new SKColor(0, 0, 0, 0));
            brush.SetFixOpacity(1);

            // background-color
            //   Optional color. Defaults to #000000. Disabled by background-pattern. Transitionable.
            //   The color with which the background will be drawn.
            if (paint.BackgroundColor != null)
            {
                if (paint.BackgroundColor.Stops != null)
                {
                    brush.SetVariableColor((context) => paint.BackgroundColor.Evaluate(context.Zoom));
                }
                else
                {
                    brush.SetFixColor(paint.BackgroundColor.SingleVal);
                }
            }

            // background-pattern
            //   Optional string. Interval.
            //   Name of image in sprite to use for drawing image background. For seamless patterns, 
            //   image width and height must be a factor of two (2, 4, 8, …, 512). Note that 
            //   zoom -dependent expressions will be evaluated only at integer zoom levels.
            if (paint.BackgroundPattern != null)
            {
                if (paint.BackgroundPattern.Stops == null && !paint.BackgroundPattern.SingleVal.Contains("{"))
                {
                    var sprite = spriteAtlas.GetSprite(paint.BackgroundPattern.SingleVal);
                    if (sprite != null && sprite.Image != null)
                        brush.SetFixShader(sprite.Image.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat));
                }
                else
                {
                    brush.SetVariableShader((context) =>
                    {
                        var name = ReplaceFields(paint.BackgroundPattern.Evaluate(context.Zoom), null);

                        var sprite = spriteAtlas.GetSprite(name);
                        if (sprite != null && sprite.Image != null)
                        {
                            return sprite.Image.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
                        }
                        else
                        {
                            // Log information
                            // TODO
                            // Logging.Logger.Log(Logging.LogLevel.Information, $"Fill pattern {name} not found");
                            // No sprite found
                            return null;
                        }
                    });
                }
            }

            // background-opacity
            //   Optional number. Defaults to 1.
            //   The opacity at which the background will be drawn.
            if (paint?.BackgroundOpacity != null)
            {
                if (paint.BackgroundOpacity.Stops != null)
                {
                    brush.SetVariableOpacity((context) => paint.BackgroundOpacity.Evaluate(context.Zoom));
                }
                else
                {
                    brush.SetFixOpacity(paint.BackgroundOpacity.SingleVal);
                }
            }

            return new List<MGLPaint> { brush };
        }

        public static List<MGLPaint> ConvertRasterLayer(StyleLayer style)
        {
            // visibility
            //   Optional enum. One of visible, none. Defaults to visible.
            //   The display of this layer. none hides this layer.
            if (style.Layout?.Visibility != null && style.Layout.Visibility.Equals("none"))
                return null;

            var paint = style.Paint;

            var brush = new MGLPaint();

            brush.SetFixOpacity(1);

            // raster-opacity
            //   Optional number. Defaults to 1.
            //   The opacity at which the image will be drawn.
            if (paint?.RasterOpacity != null)
            {
                if (paint.RasterOpacity.Stops != null)
                {
                    brush.SetVariableOpacity((context) => paint.RasterOpacity.Evaluate(context.Zoom));
                }
                else
                {
                    brush.SetFixOpacity(paint.RasterOpacity.SingleVal);
                }
            }

            // raster-hue-rotate
            //   Optional number. Units in degrees. Defaults to 0.
            //   Rotates hues around the color wheel.

            // raster-brightness-min
            //   Optional number.Defaults to 0.
            //   Increase or reduce the brightness of the image. The value is the minimum brightness.

            // raster-brightness-max
            //   Optional number. Defaults to 1.
            //   Increase or reduce the brightness of the image. The value is the maximum brightness.

            // raster-saturation
            //   Optional number.Defaults to 0.
            //   Increase or reduce the saturation of the image.

            // raster-contrast
            //   Optional number. Defaults to 0.
            //   Increase or reduce the contrast of the image.

            // raster-fade-duration
            //   Optional number.Units in milliseconds.Defaults to 300.
            //   Fade duration when a new tile is added.

            return new List<MGLPaint>() { brush };
        }

        public static List<MGLPaint> ConvertFillLayer(StyleLayer layer, MGLSpriteAtlas spriteAtlas)
        {
            var layout = layer?.Layout;
            var paint = layer?.Paint;

            // visibility
            //   Optional enum. One of visible, none. Defaults to visible.
            //   The display of this layer. none hides this layer.
            if (layout?.Visibility != null && layout.Visibility.Equals("none"))
            {
                return null;
            }

            var area = new MGLPaint();
            var line = new MGLPaint();

            // Set defaults
            area.SetFixColor(new SKColor(0, 0, 0, 0));
            area.SetFixOpacity(1);
            area.SetFixStyle(SKPaintStyle.Fill);
            line.SetFixColor(new SKColor(0, 0, 0, 0));
            line.SetFixOpacity(1);
            line.SetFixStyle(SKPaintStyle.Stroke);
            line.SetFixStrokeWidth(0);

            // If we don't have a paint, than there isn't anything that we could do
            if (paint == null)
            {
                return new List<MGLPaint>() { area, line };
            }

            // fill-color
            //   Optional color. Defaults to #000000. Disabled by fill-pattern. Exponential.
            //   The color of the filled part of this layer. This color can be specified as 
            //   rgba with an alpha component and the color's opacity will not affect the 
            //   opacity of the 1px stroke, if it is used.
            if (paint.FillColor != null)
            {
                if (paint.FillColor.Stops != null)
                {
                    area.SetVariableColor((context) => layer.Paint.FillColor.Evaluate(context.Zoom));
                    line.SetVariableColor((context) => layer.Paint.FillColor.Evaluate(context.Zoom));
                }
                else
                {
                    area.SetFixColor(layer.Paint.FillColor.SingleVal);
                    line.SetFixColor(layer.Paint.FillColor.SingleVal);
                }
            }

            // fill-outline-color
            //   Optional color. Disabled by fill-pattern. Requires fill-antialias = true. Exponential. 
            //   The outline color of the fill. Matches the value of fill-color if unspecified.
            if (paint.FillOutlineColor != null && paint.FillAntialias != null)
            {
                if (paint.FillOutlineColor.Stops != null)
                {
                    line.SetVariableColor((context) => layer.Paint.FillOutlineColor.Evaluate(context.Zoom));
                }
                else
                {
                    line.SetFixColor(layer.Paint.FillOutlineColor.SingleVal);
                }
            }

            // fill-opacity
            //   Optional number. Defaults to 1. Exponential.
            //   The opacity of the entire fill layer. In contrast to the fill-color, this 
            //   value will also affect the 1px stroke around the fill, if the stroke is used.
            if (paint.FillOpacity != null)
            {
                if (paint.FillOpacity.Stops != null)
                {
                    area.SetVariableOpacity((context) => layer.Paint.FillOpacity.Evaluate(context.Zoom));
                    line.SetVariableOpacity((context) => layer.Paint.FillOpacity.Evaluate(context.Zoom));
                }
                else
                {
                    area.SetFixOpacity(layer.Paint.FillOpacity.SingleVal);
                    line.SetFixOpacity(layer.Paint.FillOpacity.SingleVal);
                }
            }

            // fill-antialias
            //   Optional boolean. Defaults to true. Interval.
            //   Whether or not the fill should be antialiased.
            if (paint.FillAntialias != null)
            {
                if (paint.FillAntialias.Stops != null)
                {
                    area.SetVariableAntialias((context) => layer.Paint.FillAntialias.Evaluate(context.Zoom));
                    line.SetVariableAntialias((context) => layer.Paint.FillAntialias.Evaluate(context.Zoom));
                }
                else
                {
                    area.SetFixAntialias(layer.Paint.FillAntialias.SingleVal == null ? false : (bool)layer.Paint.FillAntialias.SingleVal);
                    line.SetFixAntialias(layer.Paint.FillAntialias.SingleVal == null ? false : (bool)layer.Paint.FillAntialias.SingleVal);
                }
            }

            // fill-translate
            //   Optional array. Units in pixels. Defaults to 0,0. Exponential.
            //   The geometry's offset. Values are [x, y] where negatives indicate left and up, 
            //   respectively.

            // TODO: Use matrix of paint object for this

            // fill-translate-anchor
            //   Optional enum. One of map, viewport. Defaults to map. Requires fill-translate. Interval.
            //   Control whether the translation is relative to the map (north) or viewport (screen)

            // TODO: Use matrix of paint object for this

            // fill-pattern
            //   Optional string. Interval.
            //   Name of image in sprite to use for drawing image fills. For seamless patterns, 
            //   image width and height must be a factor of two (2, 4, 8, …, 512).
            if (paint.FillPattern != null)
            {
                // FillPattern needs a color. Instead no pattern is drawn.
                area.SetFixColor(SKColors.Black);

                if (paint.FillPattern.Stops == null && !paint.FillPattern.SingleVal.Contains("{"))
                {
                    var name = paint.FillPattern.SingleVal;

                    var sprite = spriteAtlas.GetSprite(paint.FillPattern.SingleVal);
                    if (sprite != null && sprite.Image != null)
                    { 
                        area.SetFixShader(sprite.Image.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat));
                    }
                    else
                    {
                        // Log information, that no sprite is found
                        // TODO
                        // Logging.Logger.Log(Logging.LogLevel.Information, $"Fill pattern {name} not found");
                    }
                }
                else
                {
                    area.SetVariableShader((context) =>
                    {
                        var name = ReplaceFields(layer.Paint.FillPattern.Evaluate(context.Zoom), context.Tags);

                        var sprite = spriteAtlas.GetSprite(name);
                        if (sprite != null && sprite.Image != null)
                        {
                            return sprite.Image.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
                        }
                        else
                        {
                            // Log information, that no sprite is found
                            // TODO
                            // Logging.Logger.Log(Logging.LogLevel.Information, $"Fill pattern {name} not found");
                            return null;
                        }
                    });
                }
            }

            return new List<MGLPaint>() { area, line };
        }

        public static List<MGLPaint> ConvertLineLayer(StyleLayer layer, MGLSpriteAtlas spriteAtlas)
        {
            var layout = layer?.Layout;
            var paint = layer?.Paint;

            // visibility
            //   Optional enum. One of visible, none. Defaults to visible.
            //   The display of this layer. none hides this layer.
            if (layout?.Visibility != null && layout.Visibility.Equals("none"))
            {
                return null;
            }

            var line = new MGLPaint();

            // Set defaults
            line.SetFixColor(new SKColor(0, 0, 0, 0));
            line.SetFixStyle(SKPaintStyle.Stroke);
            line.SetFixStrokeWidth(0);
            line.SetFixStrokeCap(SKStrokeCap.Butt);
            line.SetFixStrokeJoin(SKStrokeJoin.Miter);

            // If we don't have a paint, than there isn't anything that we could do
            if (paint == null)
            {
                return new List<MGLPaint>() { line };
            }

            // line-cap
            //   Optional enum. One of butt, round, square. Defaults to butt. Interval.
            //   The display of line endings.
            if (layout?.LineCap != null)
            {
                if (layout.LineCap.Stops != null)
                {
                    line.SetVariableStrokeCap((context) =>
                    {
                        switch (layout.LineCap.Evaluate(context.Zoom))
                        {
                            case "butt":
                                return SKStrokeCap.Butt;
                            case "round":
                                return SKStrokeCap.Round;
                            case "square":
                                return SKStrokeCap.Square;
                            default:
                                return SKStrokeCap.Butt;
                        }
                    });
                }
                else
                {
                    switch (layout.LineCap.SingleVal)
                    {
                        case "butt":
                            line.SetFixStrokeCap(SKStrokeCap.Butt);
                            break;
                        case "round":
                            line.SetFixStrokeCap(SKStrokeCap.Round);
                            break;
                        case "square":
                            line.SetFixStrokeCap(SKStrokeCap.Square);
                            break;
                        default:
                            line.SetFixStrokeCap(SKStrokeCap.Butt);
                            break;
                    }
                }
            }

            // line-join
            //   Optional enum. One of bevel, round, miter. Defaults to miter. Interval.
            //   The display of lines when joining.
            if (layout?.LineJoin != null)
            {
                if (layout.LineJoin.Stops != null)
                {
                    line.SetVariableStrokeJoin((context) =>
                    {
                        switch (layout.LineJoin.Evaluate(context.Zoom))
                        {
                            case "bevel":
                                return SKStrokeJoin.Bevel;
                            case "round":
                                return SKStrokeJoin.Round;
                            case "mitter":
                                return SKStrokeJoin.Miter;
                            default:
                                return SKStrokeJoin.Miter;
                        }
                    });
                }
                else
                {
                    switch (layout.LineJoin.SingleVal)
                    {
                        case "bevel":
                            line.SetFixStrokeJoin(SKStrokeJoin.Bevel);
                            break;
                        case "round":
                            line.SetFixStrokeJoin(SKStrokeJoin.Round);
                            break;
                        case "mitter":
                            line.SetFixStrokeJoin(SKStrokeJoin.Miter);
                            break;
                        default:
                            line.SetFixStrokeJoin(SKStrokeJoin.Miter);
                            break;
                    }
                }
            }

            // line-color
            //   Optional color. Defaults to #000000. Disabled by line-pattern. Exponential.
            //   The color with which the line will be drawn.
            if (paint?.LineColor != null)
            {
                if (paint.LineColor.Stops != null)
                {
                    line.SetVariableColor((context) => layer.Paint.LineColor.Evaluate(context.Zoom));
                }
                else
                {
                    line.SetFixColor(layer.Paint.LineColor.SingleVal);
                }
            }

            // line-width
            //   Optional number.Units in pixels.Defaults to 1. Exponential.
            //   Stroke thickness.
            if (paint?.LineWidth != null)
            {
                if (paint.LineWidth.Stops != null)
                {
                    line.SetVariableStrokeWidth((context) => layer.Paint.LineWidth.Evaluate(context.Zoom));
                }
                else
                {
                    line.SetFixStrokeWidth(layer.Paint.LineWidth.SingleVal);
                }
            }

            // line-opacity
            //   Optional number. Defaults to 1. Exponential.
            //   The opacity at which the line will be drawn.
            if (paint?.LineOpacity != null)
            {
                if (paint.LineOpacity.Stops != null)
                {
                    line.SetVariableOpacity((context) => layer.Paint.LineOpacity.Evaluate(context.Zoom));
                }
                else
                {
                    line.SetFixOpacity(layer.Paint.LineOpacity.SingleVal);
                }
            }

            // line-dasharray
            //   Optional array. Units in line widths. Disabled by line-pattern. Interval.
            //   Specifies the lengths of the alternating dashes and gaps that form the dash pattern. 
            //   The lengths are later scaled by the line width.To convert a dash length to pixels, 
            //   multiply the length by the current line width.
            if (paint?.LineDashArray != null)
            {
                if (paint.LineDashArray.Stops != null)
                {
                    line.SetVariableDashArray((context) => layer.Paint.LineDashArray.Evaluate(context.Zoom));
                }
                else
                {
                    line.SetFixDashArray(layer.Paint.LineDashArray.SingleVal);
                }
            }

            // line-miter-limit
            //   Optional number. Defaults to 2. Requires line-join = miter. Exponential.
            //   Used to automatically convert miter joins to bevel joins for sharp angles.

            // line-round-limit
            //   Optional number. Defaults to 1.05. Requires line-join = round. Exponential.
            //   Used to automatically convert round joins to miter joins for shallow angles.

            // line-translate
            //   Optional array. Units in pixels.Defaults to 0,0. Exponential.
            //   The geometry's offset. Values are [x, y] where negatives indicate left and up, 
            //   respectively.

            // line-translate-anchor
            //   Optional enum. One of map, viewport.Defaults to map. Requires line-translate. Interval.
            //   Control whether the translation is relative to the map (north) or viewport (screen)

            // line-gap-width
            //   Optional number.Units in pixels.Defaults to 0. Exponential.
            //   Draws a line casing outside of a line's actual path.Value indicates the width of 
            //   the inner gap.

            // line-offset
            //   Optional number. Units in pixels. Defaults to 0. Exponential.
            //   The line's offset perpendicular to its direction. Values may be positive or negative, 
            //   where positive indicates "rightwards" (if you were moving in the direction of the line) 
            //   and negative indicates "leftwards".

            // line-blur
            //   Optional number. Units in pixels.Defaults to 0. Exponential.
            //   Blur applied to the line, in pixels.

            // line-pattern
            //   Optional string. Interval.
            //   Name of image in sprite to use for drawing image lines. For seamless patterns, image 
            //   width must be a factor of two (2, 4, 8, …, 512).

            return new List<MGLPaint>() { line };
        }

        public static List<MGLPaint> ConvertSymbolLayer(StyleLayer styleLayer, MGLSpriteAtlas spriteAtlas)
        {
            string styleLabelText = string.Empty;
            List<MGLPaint> result = new List<MGLPaint>();

/*            if (context.Feature.GeometryType == GeometryType.LineString)
                styleLabelText = "";

            //return result;


            // visibility
            //   Optional enum. One of visible, none. Defaults to visible.
            //   The display of this layer. none hides this layer.
            if (styleLayer.Layout?.Visibility != null && styleLayer.Layout.Visibility.Equals("none"))
                return result;

            var paint = styleLayer.Paint;
            var layout = styleLayer.Layout;

            var styleLabel = new LabelStyle
            {
                Enabled = false,
                Halo = new Pen { Color = Color.Transparent, Width = 0 },
                CollisionDetection = true,
                BackColor = null,
            };

            styleLabel.Font.Size = 16;

            var styleIcon = new SymbolStyle
            {
                Enabled = false,
            };

            // symbol-placement
            //   Optional enum. One of point, line. Defaults to point. Interval.
            //   Label placement relative to its geometry. line can only be used on 
            //   LineStrings and Polygons.
            if (layout?.SymbolPlacement != null)
            {
                switch (layout.SymbolPlacement.Evaluate(context.Zoom))
                {
                    case "point":
                        break;
                    case "line":
                        // symbol-spacing
                        //   Optional number. Units in pixels. Defaults to 250. Requires symbol-placement = line. Exponential.
                        //   Distance between two symbol anchors.
                        if (layout?.SymbolSpacing != null)
                        {
                            styleLabel.Spacing = layout.SymbolSpacing.Evaluate(context.Zoom);
                        }
                        break;
                }
            }
            // symbol-avoid-edges
            //   Optional boolean. Defaults to false. Interval.
            //   If true, the symbols will not cross tile edges to avoid mutual collisions.
            //   Recommended in layers that don't have enough padding in the vector tile to prevent 
            //   collisions, or if it is a point symbol layer placed after a line symbol layer.

            // text-field
            //   Optional string. Interval.
            //   Value to use for a text label. Feature properties are specified using tokens like {field_name}.
            if (layout?.TextField != null)
            {
                styleLabelText = ReplaceFields(layout.TextField.Trim(), context.Feature.Tags);

                // text-transform
                //   Optional enum. One of none, uppercase, lowercase. Defaults to none. Requires text-field. Interval.
                //   Specifies how to capitalize text, similar to the CSS text-transform property.
                if (layout?.TextTransform != null)
                {
                    switch (layout.TextTransform)
                    {
                        case "uppercase":
                            styleLabelText = styleLabelText.ToUpper();
                            break;
                        case "lowercase":
                            styleLabelText = styleLabelText.ToLower();
                            break;
                    }
                }

                styleLabel.Text = styleLabelText;

                // text-color
                //   Optional color. Defaults to #000000. Requires text-field. Exponential.
                //   The color with which the text will be drawn.
                if (paint?.TextColor != null)
                {
                    styleLabel.ForeColor = paint.TextColor.Evaluate(context.Zoom);
                }

                // text-opacity
                //   Optional number. Defaults to 1. Requires text-field. Exponential.
                //   The opacity at which the text will be drawn.
                if (paint?.TextOpacity != null)
                {
                }

                // text-halo-color
                //   Optional color. Defaults to rgba(0, 0, 0, 0). Requires text-field. Exponential.
                //   The color of the text's halo, which helps it stand out from backgrounds.
                if (paint?.TextHaloColor != null)
                {
                    styleLabel.Halo.Color = paint.TextHaloColor.Evaluate(context.Zoom);
                }

                //text-halo-width
                //   Optional number. Units in pixels. Defaults to 0. Requires text-field. Exponential.
                //   Distance of halo to the font outline. Max text halo width is 1/4 of the font-size.
                if (paint?.TextHaloWidth != null)
                {
                    styleLabel.Halo.Width = paint.TextHaloWidth.Evaluate(context.Zoom);
                }

                // text-font
                //   Optional array. Defaults to Open Sans Regular, Arial Unicode MS Regular. Requires text-field. Interval.
                //   Font stack to use for displaying text.
                if (layout?.TextFont != null)
                {
                    var fontName = string.Empty;

                    foreach (var font in layout.TextFont)
                    {
                        // TODO: Check for fonts
                        //if (font.exists)
                        {
                            fontName = (string)font;
                            break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(fontName))
                        styleLabel.Font.FontFamily = fontName;
                }

                // text-size
                //   Optional number. Units in pixels. Defaults to 16. Requires text-field. Exponential.
                //   Font size.
                if (layout?.TextSize != null)
                {
                    styleLabel.Font.Size = layout.TextSize.Evaluate(context.Zoom);
                }

                // text-rotation-alignment
                //   Optional enum. One of map, viewport. Defaults to viewport. Requires text-field. Interval.
                //   Orientation of text when map is rotated.

                // text-translate
                //   Optional array. Units in pixels. Defaults to 0, 0. Requires text-field. Exponential.
                //   Distance that the text's anchor is moved from its original placement.Positive values 
                //   indicate right and down, while negative values indicate left and up.
                if (paint?.TextTranslate != null)
                {
                    var offset = new Offset
                    {
                        X = paint.TextTranslate.Count > 0 ? -paint.TextTranslate[0] : 0,
                        Y = paint.TextTranslate.Count > 1 ? -paint.TextTranslate[1] : 0
                    };

                    styleLabel.Offset = offset;

                    // text-translate-anchor
                    //   Optional enum. One of map, viewport. Defaults to map. Requires text-field. Requires text-translate. Interval.
                    //   Control whether the translation is relative to the map(north) or viewport(screen).

                    // TODO: Don't know, how to do this in the moment
                }

                // text-max-width
                //   Optional number. Units in em. Defaults to 10. Requires text-field. Exponential.
                //   The maximum line width for text wrapping.
                if (layout?.TextMaxWidth != null)
                {
                    styleLabel.MaxWidth = layout.TextMaxWidth.Evaluate(context.Zoom);
                }

                // text-line-height
                //   Optional number. Units in em. Defaults to 1.2. Requires text-field. Exponential.
                //   Text leading value for multi-line text.

                // text-letter-spacing
                //   Optional number. Units in em. Defaults to 0. Requires text-field. Exponential.
                //   Text tracking amount.

                // text-justify
                //   Optional enum. One of left, center, right. Defaults to center. Requires text-field. Interval.
                //   Text justification options.
                if (layout?.TextJustify != null)
                {
                    switch (layout.TextJustify)
                    {
                        case "left":
                            styleLabel.Justify = LabelStyle.HorizontalAlignmentEnum.Left;
                            break;
                        case "right":
                            styleLabel.Justify = LabelStyle.HorizontalAlignmentEnum.Right;
                            break;
                        default:
                            styleLabel.Justify = LabelStyle.HorizontalAlignmentEnum.Center;
                            break;
                    }
                }

                // text-anchor
                //   Optional enum. One of center, left, right, top, bottom, top-left, top-right, bottom-left, 
                //   bottom-right. Defaults to center. Requires text-field. Interval.
                //   Part of the text placed closest to the anchor.
                if (layout?.TextAnchor != null)
                {
                    switch (layout.TextAnchor)
                    {
                        case "left":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
                            break;
                        case "right":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right;
                            break;
                        case "top":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Top;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
                            break;
                        case "bottom":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
                            break;
                        case "top-left":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Top;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
                            break;
                        case "top-right":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Top;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right;
                            break;
                        case "bottom-left":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
                            break;
                        case "bottom-right":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right;
                            break;
                        default:
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
                            break;
                    }
                }

                // text-max-angle
                //   Optional number. Units in degrees. Defaults to 45. Requires text-field. 
                //   Requires symbol-placement = line. Exponential.
                //   Maximum angle change between adjacent characters.

                // text-rotate
                //   Optional number. Units in degrees. Defaults to 0. Requires text-field. Exponential.
                //   Rotates the text clockwise.

                // text-padding
                //   Optional number. Units in pixels. Defaults to 2. Requires text-field. Exponential.
                //   Size of the additional area around the text bounding box used for detecting symbol collisions.

                // text-keep-upright
                //   Optional boolean. Defaults to true. Requires text-field. Requires text-rotation-alignment = map.
                //   Requires symbol-placement = line. Interval.
                //   If true, the text may be flipped vertically to prevent it from being rendered upside-down.

                // text-offset
                //   Optional array. Units in ems. Defaults to 0,0. Requires text-field. Exponential.
                //   Offset distance of text from its anchor. Positive values indicate right and down, 
                //   while negative values indicate left and up.
                if (layout?.TextOffset != null)
                {
                    var x = layout.TextOffset[0] * styleLabel.Font.Size;
                    var y = layout.TextOffset[1] * styleLabel.Font.Size;
                    styleLabel.Offset = new Offset(x, y, false);
                }

                // text-allow-overlap
                //   Optional boolean. Defaults to false. Requires text-field. Interval.
                //   If true, the text will be visible even if it collides with other previously drawn symbols.
                if (layout?.TextAllowOverlap != null)
                {
                    // TODO
                    layout.TextAllowOverlap.Evaluate(context.Zoom);
                }

                // text-ignore-placement
                //   Optional boolean. Defaults to false. Requires text-field. Interval.
                //   If true, other symbols can be visible even if they collide with the text.
                if (layout?.TextIgnorePlacement != null)
                {
                    // TODO
                    layout.TextIgnorePlacement.Evaluate(context.Zoom);
                }

                // text-optional
                //   Optional boolean. Defaults to false. Requires text-field. Requires icon-image. Interval.
                //   If true, icons will display without their corresponding text when the text collides with other symbols and the icon does not.
                if (layout?.TextOptional != null)
                {
                    // TODO
                    layout.TextOptional.Evaluate(context.Zoom);
                }

                // text-halo-blur
                //   Optional number. Units in pixels. Defaults to 0. Requires text-field. Exponential.
                //   The halo's fadeout distance towards the outside.
            }

            // icon-image
            //   Optional string.
            //   A string with { tokens } replaced, referencing the data property to pull from. Interval.
            if (layout?.IconImage != null)
            {
                var name = ReplaceFields(layout.IconImage.Evaluate(context.Zoom), context.Feature.Tags);

                if (!string.IsNullOrEmpty(name) && spriteAtlas.ContainsKey(name) && spriteAtlas[name].Atlas >= 0)
                {
                    styleIcon.BitmapId = spriteAtlas[name].Atlas;
                }
                else
                {
                    // No sprite found
                    styleIcon.BitmapId = -1;
                    // Log information
                    Logging.Logger.Log(Logging.LogLevel.Information, $"Sprite {name} not found");
                }

                // icon-allow-overlap
                //   Optional boolean. Defaults to false. Requires icon-image. Interval.
                //   If true, the icon will be visible even if it collides with other previously drawn symbols.
                if (layout?.IconAllowOverlap != null)
                {
                    // TODO
                    layout.IconAllowOverlap.Evaluate(context.Zoom);
                }

                // icon-ignore-placement
                //   Optional boolean. Defaults to false. Requires icon-image. Interval.
                //   If true, other symbols can be visible even if they collide with the icon.
                if (layout?.IconIgnorePlacement != null)
                {
                    // TODO
                    layout.IconIgnorePlacement.Evaluate(context.Zoom);
                }

                // icon-optional
                //   Optional boolean. Defaults to false. Requires icon-image. Requires text-field. Interval.
                //   If true, text will display without their corresponding icons when the icon collides 
                //   with other symbols and the text does not.
                if (layout?.IconOptional != null)
                {
                    // TODO
                    layout.IconOptional.Evaluate(context.Zoom);
                }

                // icon-rotation-alignment
                //   Optional enum. One of map, viewport. Defaults to viewport. Requires icon-image. Interval.
                //   Orientation of icon when map is rotated.

                // icon-size
                //   Optional number. Defaults to 1. Requires icon-image. Exponential.
                //   Scale factor for icon. 1 is original size, 3 triples the size.
                if (layout?.IconSize != null)
                {
                    styleIcon.SymbolScale = layout.IconSize.Evaluate(context.Zoom);
                }

                // icon-rotate
                //   Optional number. Units in degrees. Defaults to 0. Requires icon-image. Exponential.
                //   Rotates the icon clockwise.

                // icon-padding
                //   Optional number. Units in pixels. Defaults to 2. Requires icon-image. Exponential.
                //   Size of the additional area around the icon bounding box used for detecting symbol collisions.

                // icon-keep-upright
                //   Optional boolean. Defaults to false. Requires icon-image. Requires icon-rotation-alignment = map. Interval.
                //   Requires symbol-placement = line.
                //   If true, the icon may be flipped to prevent it from being rendered upside-down.

                // icon-offset
                //   Optional array. Defaults to 0,0. Requires icon-image. Exponential.
                //   Offset distance of icon from its anchor. Positive values indicate right and down, 
                //   while negative values indicate left and up.
                if (layout?.IconOffset != null)
                {
                    var x = layout.IconOffset[0];
                    var y = layout.IconOffset[1];
                    styleIcon.SymbolOffset = new Offset(x, y, false);
                }

                // icon-opacity
                //   Optional number. Defaults to 1. Requires icon-image. Exponential.
                //   The opacity at which the icon will be drawn.
                if (layout?.IconOpacity != null)
                {
                    styleIcon.Opacity = layout.IconOpacity.Evaluate(context.Zoom);
                }

                // icon-color
                //   Optional color. Defaults to #000000. Requires icon-image. Exponential.
                //   The color of the icon. This can only be used with sdf icons.

                // icon-halo-color
                //   Optional color. Defaults to rgba(0, 0, 0, 0). Requires icon-image. Exponential.
                //   The color of the icon's halo. Icon halos can only be used with sdf icons.

                // icon-halo-width
                //   Optional number. Units in pixels. Defaults to 0. Requires icon-image. Exponential.
                //   Distance of halo to the icon outline.

                // icon-halo-blur
                //   Optional number. Units in pixels. Defaults to 0. Requires icon-image. Exponential.
                //   Fade out the halo towards the outside.

                // icon-translate
                //   Optional array. Units in pixels. Defaults to 0, 0. Requires icon-image. Exponential.
                //   Distance that the icon's anchor is moved from its original placement.
                //   Positive values indicate right and down, while negative values indicate left and up.

                // icon-translate-anchor
                //   Optional enum. One of map, viewport. Defaults to map. Requires icon-image. Requires icon-translate. Interval.
                //   Control whether the translation is relative to the map(north) or viewport(screen).
            }

            if (!string.IsNullOrEmpty(styleLabelText))
            {
                styleLabel.Enabled = true;

                result.Add(styleLabel);
            }

            if (styleIcon.BitmapId >= 0)
            {
                styleIcon.Enabled = true;

                result.Add(styleIcon);
            }

            if (symbolProvider != null)
            {
                symbolProvider.Add(new Symbol(context.Feature, styleIcon, styleLabel, styleLayer.ZIndex));

                // If there is an SymbolLayer, than it handles drawing of symbols
                result = null;
            }
*/
            return result;
        }

        static Regex regExFields = new Regex(@"\{(.*?)\}", (RegexOptions)8);

        /// <summary>
        /// Replace all fields in string with values
        /// </summary>
        /// <param name="text">String with fields to replace</param>
        /// <param name="tags">Tags to replace fields with</param>
        /// <returns></returns>
        public static string ReplaceFields(string text, TagsCollection tags)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var result = text;

            var match = regExFields.Match(text);

            while (match.Success)
            {
                var field = match.Groups[1].Captures[0].Value;

                // Search field
                var replacement = string.Empty;

                if (tags.ContainsKey(field))
                    replacement = tags[field].ToString();

                // Replace field with new value
                result = result.Replace(match.Groups[0].Captures[0].Value, replacement);

                // Check for next field
                match = match.NextMatch();
            };

            return result;
        }
    }
}
