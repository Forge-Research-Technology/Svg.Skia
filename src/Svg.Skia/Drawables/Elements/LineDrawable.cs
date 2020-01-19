﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Svg.Skia
{
    public class LineDrawable : DrawablePath
    {
        public LineDrawable(SvgLine svgLine, SKRect skOwnerBounds, IgnoreAttributes ignoreAttributes = IgnoreAttributes.None)
        {
            IgnoreAttributes = ignoreAttributes;
            IsDrawable = CanDraw(svgLine, IgnoreAttributes);

            if (!IsDrawable)
            {
                return;
            }

            Path = svgLine.ToSKPath(svgLine.FillRule, skOwnerBounds, _disposable);
            if (Path == null || Path.IsEmpty)
            {
                IsDrawable = false;
                return;
            }

            IsAntialias = SKPaintExtensions.IsAntialias(svgLine);

            TransformedBounds = Path.Bounds;

            Transform = SKMatrixExtensions.ToSKMatrix(svgLine.Transforms);

            ClipPath = SvgClipPathExtensions.GetSvgVisualElementClipPath(svgLine, TransformedBounds, new HashSet<Uri>(), _disposable);
            MaskDrawable = SvgMaskExtensions.GetSvgVisualElementMask(svgLine, TransformedBounds, new HashSet<Uri>(), _disposable);
            if (MaskDrawable != null)
            {
                CreateMaskPaints();
            }
            Opacity = ignoreAttributes.HasFlag(IgnoreAttributes.Opacity) ? null : SKPaintExtensions.GetOpacitySKPaint(svgLine, _disposable);
            Filter = ignoreAttributes.HasFlag(IgnoreAttributes.Filter) ? null : SvgFiltersExtensions.GetFilterSKPaint(svgLine, TransformedBounds, _disposable);

            if (SKPaintExtensions.IsValidFill(svgLine))
            {
                Fill = SKPaintExtensions.GetFillSKPaint(svgLine, TransformedBounds, ignoreAttributes, _disposable);
                if (Fill == null)
                {
                    IsDrawable = false;
                    return;
                }
            }

            if (SKPaintExtensions.IsValidStroke(svgLine, TransformedBounds))
            {
                Stroke = SKPaintExtensions.GetStrokeSKPaint(svgLine, TransformedBounds, ignoreAttributes, _disposable);
                if (Stroke == null)
                {
                    IsDrawable = false;
                    return;
                }
            }

            CreateMarkers(svgLine, Path, skOwnerBounds);

            // TODO: Transform _skBounds using _skMatrix.
            SKMatrix.MapRect(ref Transform, out TransformedBounds, ref TransformedBounds);
        }
    }
}
