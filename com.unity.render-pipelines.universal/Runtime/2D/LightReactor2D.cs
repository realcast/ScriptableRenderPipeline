using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Rendering.Universal
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("Rendering/2D/Light Reactor 2D (Experimental)")]
    public class LightReactor2D : ShadowCasterGroup2D
    {
        [SerializeField] bool m_HasRenderer = false;
        [SerializeField] bool m_UseRendererSilhouette = true;
        [SerializeField] bool m_CastsShadows = true;
        [SerializeField] bool m_SelfShadows = false;
        [SerializeField] int[] m_ApplyToSortingLayers = null;
        [SerializeField] Vector3[] m_ShapePath;
        [SerializeField] int m_ShapePathHash = 0;
        [SerializeField] int m_PreviousPathHash = 0;
        [SerializeField] Mesh m_Mesh;

        internal ShadowCasterGroup2D m_ShadowCasterGroup = null;
        internal ShadowCasterGroup2D m_PreviousShadowCasterGroup = null;

        internal Mesh mesh => m_Mesh;
        internal Vector3[] shapePath => m_ShapePath;
        internal int shapePathHash { get { return m_ShapePathHash; } set { m_ShapePathHash = value; } }

        internal int[] applyToSortingLayers => m_ApplyToSortingLayers;

        Mesh m_ShadowMesh;
        int m_PreviousShadowGroup = 0;
        bool m_PreviousCastsShadows = true;

        public bool useRendererSilhouette
        {
            set { m_UseRendererSilhouette = value; }
            get { return m_UseRendererSilhouette; }
        }
            
        public bool selfShadows
        {
            set { m_SelfShadows = value; }
            get { return m_SelfShadows; }
        }

        public bool castsShadows
        {
            set { m_CastsShadows = value; }
            get { return m_CastsShadows; }
        }

        static int[] SetDefaultSortingLayers()
        {
            int layerCount = SortingLayer.layers.Length;
            int[] allLayers = new int[layerCount];

            for(int layerIndex=0;layerIndex < layerCount;layerIndex++)
            {
                allLayers[layerIndex] = SortingLayer.layers[layerIndex].id;
            }

            return allLayers;
        }

        internal bool IsShadowedLayer(int layer)
        {
            return m_ApplyToSortingLayers != null ? Array.IndexOf(m_ApplyToSortingLayers, layer) >= 0 : false;
        }

        private void Awake()
        {
            if(m_ApplyToSortingLayers == null)
                m_ApplyToSortingLayers = SetDefaultSortingLayers();

            Bounds bounds = new Bounds(transform.position, Vector3.one);
            
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                bounds = renderer.bounds;
            }
            else
            {
                Collider2D collider = GetComponent<Collider2D>();
                if (collider != null)
                    bounds = collider.bounds;
            }

            Vector3 relOffset = bounds.center - transform.position;

            if (m_ShapePath == null || m_ShapePath.Length == 0)
                m_ShapePath = new Vector3[] { relOffset + new Vector3(-bounds.extents.x, -bounds.extents.y), relOffset + new Vector3(bounds.extents.x, -bounds.extents.y), relOffset + new Vector3(bounds.extents.x, bounds.extents.y), relOffset + new Vector3(-bounds.extents.x, bounds.extents.y)};
        }

        protected void OnEnable()
        {
            if (m_Mesh == null)
            {
                m_Mesh = new Mesh();
                ShadowUtility.GenerateShadowMesh(ref m_Mesh, m_ShapePath);
                m_PreviousPathHash = m_ShapePathHash;
            }

            m_ShadowCasterGroup = null;
        }

        protected void OnDisable()
        {
            LightUtility.RemoveLightReactorFromGroup(this, m_ShadowCasterGroup);
        }

        public void Update()
        {
            Renderer renderer = GetComponent<Renderer>();
            m_HasRenderer = renderer != null;
            if (!m_HasRenderer)
                m_UseRendererSilhouette = false;

            bool rebuildMesh = false;
            rebuildMesh |= LightUtility.CheckForChange(m_ShapePathHash, ref m_PreviousPathHash);

            if (rebuildMesh)
                ShadowUtility.GenerateShadowMesh(ref m_Mesh, m_ShapePath);

            m_PreviousShadowCasterGroup = m_ShadowCasterGroup;
            bool addedToNewGroup = LightUtility.AddToLightReactorToGroup(this, ref m_ShadowCasterGroup);
            if (addedToNewGroup && m_ShadowCasterGroup != null)
            {
                if (m_PreviousShadowCasterGroup == this)
                    ShadowCasterGroup2DManager.RemoveGroup(this);

                LightUtility.RemoveLightReactorFromGroup(this, m_PreviousShadowCasterGroup);
                if (m_ShadowCasterGroup == this)
                    ShadowCasterGroup2DManager.AddGroup(this);
            }

            if (LightUtility.CheckForChange(m_ShadowGroup, ref m_PreviousShadowGroup))
            {
                ShadowCasterGroup2DManager.RemoveGroup(this);
                ShadowCasterGroup2DManager.AddGroup(this);
            }

            if (LightUtility.CheckForChange(m_CastsShadows, ref m_PreviousCastsShadows))
            {
                if(m_CastsShadows)
                    ShadowCasterGroup2DManager.AddGroup(this);
                else
                    ShadowCasterGroup2DManager.RemoveGroup(this);
            }
        }
    }
}
