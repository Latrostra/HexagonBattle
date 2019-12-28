using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    private int elevation;
    public int Elevation {
        get { return elevation; }
        set { 
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetric.elevationStep;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = elevation * -HexMetric.elevationStep;
            uiRect.localPosition = uiPosition;
        } 
    }
    public Color color;
    public RectTransform uiRect;
    [SerializeField] private HexCell[] neighboursHex;
    public HexCell GetNeighbour(HexDirection direction) {
        return neighboursHex[(int)direction];
    }
    public void SetNeighbour(HexDirection direction, HexCell cell) {
        neighboursHex[(int)direction] = cell;
        cell.neighboursHex[(int)direction.Opposite()] = this;
    }
}
