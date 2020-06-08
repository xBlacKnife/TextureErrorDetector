﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Wood,
    TreeSappling,
    Bucket,
    Bone,
    Cow,
    None
}

public class Item : Interactable
{
    [Header("Item")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private GameObject itemPrefab;
    [Range(0f, 10f)] [SerializeField] protected float weight;

    public ItemType GetItemType() { return itemType; }
    public float GetWeight() { return weight; }

    public override void Interact()
    {
        if (!PlayerController.Instance.HasItem())
        {
            Transform itemToHold = Instantiate(itemPrefab).transform;
            PlayerController.Instance.HoldItem(itemToHold);
            LevelManager.Instance.PerformAction(ActionType.PickUpItem);

			if (itemType == ItemType.Bone && !LevelManager.Instance.levelBoneInteracted)
			{
				LevelManager.Instance.levelBoneInteracted = true;
				if (Tracker.Instance.UseTrackerSystem)
					Tracker.Instance.TrackEvent(EventName.BONE_FOUND, EventType.Time_Stamp_Event, System.DateTime.Now.ToString(), Globals.actualLevel);
			}

            Destroy(this.gameObject);
        }
    }
}
