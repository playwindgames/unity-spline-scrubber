using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Splines;

namespace SplineScrubber
{
    /* TODO
     * edit vs playmode
     * cache on start or serializing
     */
    [ExecuteAlways]
    public class SplineClipData : MonoBehaviour
    {
        [SerializeField] private SplineContainer _container;

        public ISplineEvaluate JobHandler => _handler;
        public float Length => _length;
        public SplinePath<Spline> SplinePath => _path;

        private ISplineEvaluate _handler;
        private float _length;
        private SplinePath<Spline> _path;
        private NativeSpline _nativeSpline;

        private void Awake()
        {
            _handler = GetComponent<ISplineEvaluate>();
        }

        private void Start()
        {
            if (!Application.isPlaying) return;
            
            SplinesMoveHandler.Instance.Register(_handler);
            _handler.Spline = _nativeSpline;
        }

        private void OnEnable()
        {
            if (_container == null)
            {
                Debug.LogError("Missing SplineContainer");
                enabled = false;
                return;
            }
            
            Spline.Changed += OnSplineChanged;
            // EditorSplineUtility.AfterSplineWasModified += OnSplineModified;
            CacheData();
        }

        private void OnDisable()
        {
            Spline.Changed -= OnSplineChanged;
            // EditorSplineUtility.AfterSplineWasModified -= OnSplineModified;
            
            Dispose();
        }
        
        private void OnSplineChanged(Spline spline, int _, SplineModification __)
        {
            OnSplineModified(spline);
        }
        
        private void OnSplineModified(Spline spline)
        {
            if (_container.Spline != spline)
            {
                return;
            }
            
            CacheData();
        }

        private void CacheData()
        {
            _length = _container.CalculateLength();
            _path = new SplinePath<Spline>(_container.Splines);
            _nativeSpline = new NativeSpline(_path, _container.transform.localToWorldMatrix, Allocator.Persistent);
        }

        private void Dispose()
        {
            _nativeSpline.Dispose();
        }
    }
}