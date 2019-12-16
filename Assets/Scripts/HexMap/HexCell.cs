using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public Color color;
    [SerializeField] private HexCell[] neighboursHex;
    public HexCell GetNeighbour(HexDirection direction) {
        return neighboursHex[(int)direction];
    }
    public void SetNeighbour(HexDirection direction, HexCell cell) {
        neighboursHex[(int)direction] = cell;
        cell.neighboursHex[(int)direction.Opposite()] = this;
    }
}
