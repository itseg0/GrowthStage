using UnityEngine;
using System;

public class Item : MonoBehaviour
{
    [ItemCodeDescription]
    [SerializeField]
    private int _itemCode;

    private SpriteRenderer spriteRenderer;
    private Sprite _itemSprite;

    public int ItemCode { get { return _itemCode; } set { _itemCode = value; } }
    public ItemType ItemType { get; set; }
    public Sprite ItemSprite { get { return _itemSprite; } set { _itemSprite = value; } }

    public Guid UniqueIdentifier { get; set; }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (ItemCode != 0)
        {
            Init(ItemCode);
        }
    }

    public void Init(int itemCodeParam)
    {
        _itemCode = itemCodeParam;
        _itemSprite = spriteRenderer.sprite;
    }

    public void SetUniqueIdentifier(Guid identifier)
    {
        UniqueIdentifier = identifier;
    }
}
