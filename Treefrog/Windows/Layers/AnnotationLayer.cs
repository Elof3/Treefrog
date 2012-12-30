﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation.Layers;
using System.Collections.ObjectModel;
using Treefrog.Presentation.Annotations;
using Treefrog.Windows.Annotations;
using System.Collections.Specialized;

namespace Treefrog.Windows.Layers
{
    public class AnnotationLayer : RenderLayer
    {
        private AnnotationLayerPresenter _model;

        protected override void DisposeManaged ()
        {
            ClearAnnotationCache();
            Model = null;

            base.DisposeManaged();
        }

        protected override void RenderContent (SpriteBatch spriteBatch)
        {
            float zoom = 1f;
            if (LevelGeometry != null)
                zoom = LevelGeometry.ZoomFactor;

            foreach (AnnotationRenderer renderer in _annotCache.Values)
                renderer.Render(spriteBatch, zoom);
        }

        public new AnnotationLayerPresenter Model
        {
            get { return _model; }
            set 
            {
                if (_model != null) {
                    _model.AnnotationsCollectionChanged -= ModelAnnotationsCollectionChanged;
                }

                _model = value;
                if (_model != null) {
                    _model.AnnotationsCollectionChanged += ModelAnnotationsCollectionChanged;
                    BindAnnotations(_model.Annotations);
                }
                else {
                    BindAnnotations(null);
                }
            }
        }

        private void ModelAnnotationsCollectionChanged (object sender, EventArgs e)
        {
            BindAnnotations(_model.Annotations);
        }

        private void BindAnnotations (ObservableCollection<Annotation> annotations)
        {
            if (_annotations != null) {
                _annotations.CollectionChanged -= HandleAnnotCollectionChanged;
            }

            ClearAnnotationCache();

            _annotations = annotations;
            if (_annotations != null) {
                _annotations.CollectionChanged += HandleAnnotCollectionChanged;
                InitializeAnnotationCache();
            }
        }

        #region Annotation Management

        private ObservableCollection<Annotation> _annotations;
        private Dictionary<Annotation, AnnotationRenderer> _annotCache = new Dictionary<Annotation, AnnotationRenderer>();

        private void InitializeAnnotationCache ()
        {
            ClearAnnotationCache();

            foreach (Annotation item in _annotations)
                AddToAnnotationCache(item);
        }

        private void AddToAnnotationCache (Annotation key)
        {
            if (_annotCache.ContainsKey(key))
                RemoveFromAnnotationCache(key);

            _annotCache.Add(key, AnnotationRendererFactory.Create(key));
        }

        private void RemoveFromAnnotationCache (Annotation key)
        {
            AnnotationRenderer renderer;
            if (_annotCache.TryGetValue(key, out renderer))
                renderer.Dispose();

            _annotCache.Remove(key);
        }

        private void ClearAnnotationCache ()
        {
            foreach (AnnotationRenderer renderer in _annotCache.Values)
                if (renderer != null)
                    renderer.Dispose();

            _annotCache.Clear();
        }

        private void HandleAnnotCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (Annotation item in e.NewItems)
                        AddToAnnotationCache(item);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Annotation item in e.OldItems)
                        RemoveFromAnnotationCache(item);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ClearAnnotationCache();
                    foreach (Annotation item in sender as ObservableCollection<Annotation>)
                        AddToAnnotationCache(item);
                    break;
            }
        }

        #endregion
    }
}
