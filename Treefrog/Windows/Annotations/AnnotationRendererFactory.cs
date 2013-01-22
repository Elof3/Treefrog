﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Presentation.Annotations;
using Treefrog.Utility;

namespace Treefrog.Windows.Annotations
{
    public class AnnotationRendererFactory : DependentTypeFactory<Annotation, AnnotationRenderer>
    {
        public static AnnotationRendererFactory Default { get; private set; }

        static AnnotationRendererFactory ()
        {
            Default = new AnnotationRendererFactory();

            Default.Register<SelectionAnnot, SelectionAnnotRenderer>(annot => {
                return new SelectionAnnotRenderer(annot as SelectionAnnot);
            });
            Default.Register<MultiTileSelectionAnnot, MultiTileSelectionAnnotRenderer>(annot => {
                return new MultiTileSelectionAnnotRenderer(annot as MultiTileSelectionAnnot);
            });
            Default.Register<CircleAnnot, CircleAnnotRenderer>(annot => {
                return new CircleAnnotRenderer(annot as CircleAnnot);
            });
        }
    }
}
