﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.ViewModel.Annotations;

namespace Treefrog.Controls.Annotations
{
    public class AnnotationRendererFactory
    {
        public static AnnotationRenderer Create (Annotation annot)
        {
            if (annot.GetType() == typeof(SelectionAnnot))
                return new SelectionAnnotRenderer(annot as SelectionAnnot);
            else if (annot.GetType() == typeof(MultiTileSelectionAnnot))
                return new MultiTileSelectionAnnotRenderer(annot as MultiTileSelectionAnnot);

            return null;
        }
    }
}
