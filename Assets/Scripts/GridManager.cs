using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] float _QuantityX;
    [SerializeField] float _QuantityY;
    //[SerializeField] Sprite _sprite;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform _cam;
    // Start is called before the first frame update
    void Start()
    {
        GeneratateBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GeneratateBoard()
    {
        float _width = _QuantityX * 2;
        float _height= _QuantityY* 2;
        for (float x = 0; x < _QuantityX; x++)
        {
            for (float y = 0; y < _QuantityY; y++)
            {
                var spawnTile=Instantiate(cellPrefab, 2*new Vector2(x, y), Quaternion.identity);
                spawnTile.name = $"Tile {x} {y}";
            }
        }
        _cam.transform.position = new Vector3(_width / 2-1f, _height / 2-3f, -10);
    }
}
