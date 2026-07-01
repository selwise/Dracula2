using System;
using UnityEngine;

[ExecuteAlways]
public sealed class CastleGalleryPrototypeLayout : MonoBehaviour
{
    [Serializable]
    public sealed class Placement
    {
        public string label;
        public Transform target;
        public bool editPosition = true;
        public Vector3 position;
        public bool editRotation;
        public Vector3 eulerAngles;
        public bool editScale;
        public Vector3 scale = Vector3.one;
        [SerializeField, HideInInspector] private Vector3 lastAppliedPosition;
        [SerializeField, HideInInspector] private Vector3 lastAppliedEulerAngles;
        [SerializeField, HideInInspector] private Vector3 lastAppliedScale = Vector3.one;

        public Placement(string label, Transform target, bool editScale, bool editRotation = false)
        {
            this.label = label;
            this.target = target;
            this.editScale = editScale;
            this.editRotation = editRotation;
            Capture();
        }

        public void Capture()
        {
            if (target == null)
            {
                return;
            }

            position = target.position;
            eulerAngles = target.eulerAngles;
            scale = target.localScale;
            lastAppliedPosition = position;
            lastAppliedEulerAngles = eulerAngles;
            lastAppliedScale = scale;
        }

        public void Apply()
        {
            Apply(false);
        }

        public void ApplyChangedValues()
        {
            Apply(true);
        }

        private void Apply(bool changedOnly)
        {
            if (target == null)
            {
                return;
            }

            if (editPosition && (!changedOnly || position != lastAppliedPosition))
            {
                target.position = position;
                lastAppliedPosition = position;
            }

            if (editRotation && (!changedOnly || eulerAngles != lastAppliedEulerAngles))
            {
                target.rotation = Quaternion.Euler(eulerAngles);
                lastAppliedEulerAngles = eulerAngles;
            }

            if (editScale && (!changedOnly || scale != lastAppliedScale))
            {
                target.localScale = scale;
                lastAppliedScale = scale;
            }
        }
    }

    [SerializeField] private Placement[] placements = new Placement[0];

    public Placement[] Placements
    {
        get { return placements; }
    }

    public void SetPlacements(Placement[] value)
    {
        placements = value ?? new Placement[0];
        CaptureCurrent();
    }

    public void CaptureCurrent()
    {
        for (int i = 0; i < placements.Length; i++)
        {
            if (placements[i] != null)
            {
                placements[i].Capture();
            }
        }
    }

    public void ApplyToTargets()
    {
        for (int i = 0; i < placements.Length; i++)
        {
            if (placements[i] != null)
            {
                placements[i].Apply();
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        ApplyChangedValuesToTargets();
    }

    private void ApplyChangedValuesToTargets()
    {
        for (int i = 0; i < placements.Length; i++)
        {
            if (placements[i] != null)
            {
                placements[i].ApplyChangedValues();
            }
        }
    }
}
