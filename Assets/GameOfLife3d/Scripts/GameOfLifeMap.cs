using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameOfLifeMap : MonoBehaviour {

	//CameraMove CameraMoveObject_;
	//CameraMove CameraMove_ {
	//	get {
	//		if (CameraMoveObject_ == null)
	//			CameraMoveObject_ = GameObject.FindObjectOfType(typeof(CameraMove)) as CameraMove;
	//		return CameraMoveObject_;
	//	}
	//}
	
	public class SBlock {
	
		public int iNeighbors;
		public bool bIsInUpdateList;
		public bool bIsVisible;
		
		public void Reset() {
			iNeighbors = 0;
			bIsInUpdateList = false;
			bIsVisible = false;
		}
	};
	
	public int iXMax_ = 100;
	public int iYMax_ = 100;
	public int iZMax_ = 100;
	int iMaxIndex_ = 0;

	public Vector3 GetCenter() {
		return new Vector3(iXMax_ * fBlockSize_ / 2f,
		                   iYMax_ * fBlockSize_ / 2f,
		                   iZMax_ * fBlockSize_ / 2f);

	}
	
	int iGrowthState_ = 1;
	int iTotalCount_ = 0;
	
	int iTimer_ = 0;
	int iMaxTimer_ = 1;
	
	public GameObject BlockModel;
	
	//Dictionary<int, SBlock> BlockMap_;
	SBlock[] BlockMap_;
	List<int> UpdateBlocks_;
	List<int> VisibleBlocks_;
	
	Queue<GameObject> UnusedBlockQueue_;
	int iBlockQueueIndex_ = 1;
	Queue<GameObject> UseBlockList1_;
	Queue<GameObject> UseBlockList2_;
	Queue<int> UnShownBlockList_;

	GameObject[] BlockModels_;
	
	float fBlockSize_ = 0f;
	public bool bScaleWithBlockSize = false;
	
	string sXMax_ = "";
	string sYMax_ = "";
	string sZMax_ = "";
	// Use this for initialization
	void Awake () {
		UnusedBlockQueue_ = new Queue<GameObject>();

		UseBlockList1_ = new Queue<GameObject>();
		UseBlockList2_ = new Queue<GameObject>();
		
		UnShownBlockList_ = new Queue<int>();
		
		//BlockMap_ = new Dictionary<int, SBlock>();
		UpdateBlocks_ = new List<int>();
		VisibleBlocks_ = new List<int>();
		
		ResetGame();
	}
	
	void ResetGame() {
		// clean up game
		while(UseBlockList1_.Count > 0) {
			GameObject Block = UseBlockList1_.Dequeue();
			GameObject.Destroy(Block.gameObject);
		}
		while(UseBlockList2_.Count > 0) {
			GameObject Block = UseBlockList2_.Dequeue();
			GameObject.Destroy(Block.gameObject);
		}
		while(UnusedBlockQueue_.Count > 0) {
			GameObject Block = UnusedBlockQueue_.Dequeue();
			GameObject.Destroy(Block.gameObject);
		}
		if (BlockModels_ != null) {
			foreach(GameObject Block in BlockModels_) {
				if (Block != null)
					GameObject.DestroyObject(Block.gameObject);
			}
		}
		
		UnusedBlockQueue_.Clear();

		UseBlockList1_.Clear();
		UseBlockList2_.Clear();
		
		UnShownBlockList_.Clear();
		
		//BlockMap_.Clear();
		UpdateBlocks_.Clear();
		VisibleBlocks_ .Clear();

		
		// start new game
		iMaxIndex_ = iXMax_ * iYMax_ * iZMax_;

		BlockMap_ = new SBlock[iMaxIndex_];
		for (int iIndex = 0; iIndex < iMaxIndex_; ++iIndex)
			BlockMap_[iIndex] = new SBlock();
		BlockModels_ = new GameObject[iMaxIndex_];
		
		Application.targetFrameRate = 1;
		
		fBlockSize_ = 1;//bScaleWithBlockSize ? BlockModel.transform.localScale[0]
			//: 1f;
		
		iBlockQueueIndex_ = 1;
		
		int iInnerRadius = iXMax_ / 10;
		int iXMid = iXMax_ / 2;
		int iYMid = iYMax_ / 2;
		int iZMid = iZMax_ / 2;
		for (int iX = 0; iX < iXMax_; iX++) {
			for (int iY = 0; iY < iYMax_; ++iY) {
				for (int iZ = 0; iZ < iZMax_; ++iZ) {
					double dVal = Mathf.Sqrt(((iX - iXMid)* (iX - iXMid)
						+ (iY - iYMid) * (iY  - iYMid)
						+ (iZ - iZMid) * (iZ - iZMid))); 
					if (dVal > iInnerRadius)
						continue;
					int iIndex = CalcBlockIndex(iX, iY, iZ);
					if (!IsInRange(iIndex))
						continue;
					SBlock Block = GetBlock(iIndex);
					if (!Block.bIsVisible) {
						Block.bIsVisible = true;
						VisibleBlocks_.Add(iIndex);
					}
					iTotalCount_++;
				}
			}
		}

		//CameraMove_.SetCenter(GetCenter());
		//Vector3 Direction = transform.position - Camera.mainCamera.transform.position;
		//Camera.mainCamera.transform.localRotation.SetLookRotation(Direction.normalized);
		//Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, iYMax_ * fBlockSize_ / 2f, -iYMax_ * fBlockSize_ / 9f);
		//Camera.main.transform.localRotation = Quaternion.identity;
		//Camera.mainCamera.fieldOfView = iYMax_ / 1.5f;

		sXMax_ = iXMax_.ToString();
		sYMax_ = iYMax_.ToString();
		sZMax_ = iZMax_.ToString();
	}
	
	// Update is called once per frame
	void Update () {
		if (++iTimer_ < iMaxTimer_)
			return;
		iTimer_ = 0;

		int iVal = iXMax_ * iYMax_ * iZMax_;
		int iLowerBound = iVal / 100;
		int iUpperBound = iVal / 600;
		
		if (iTotalCount_ >  iLowerBound)
			iGrowthState_ = -1;	// decomposition
		if (iTotalCount_ >= iLowerBound &&
			iTotalCount_ <= iUpperBound )
			iGrowthState_ = 0;	// stability
		if (iTotalCount_ < iUpperBound)
			iGrowthState_ = 1;	// growth
		
		iTotalCount_ = 0;
		
		foreach (int iIndex in VisibleBlocks_)
			RunRule(iIndex);
		
		CompileVisibleSet();

		iTotalCount_ = VisibleBlocks_.Count;
	}
	
	void CompileVisibleSet() {
		VisibleBlocks_.Clear();
		foreach(int iIndex in UpdateBlocks_) {
			if (!IsInRange(iIndex))
				continue;
			SBlock Block = GetBlock(iIndex);
			if (IsOn(Block.bIsVisible, Block.iNeighbors)) {
				Block.bIsVisible = true;
				VisibleBlocks_.Add(iIndex);
			}
			else {
				Block.Reset();
				if (BlockModels_[iIndex] != null)
					BlockModels_[iIndex].GetComponent<Renderer>().enabled = false;
			}
	
			Block.bIsInUpdateList = false;
			Block.iNeighbors = 0;
		}
		UpdateBlocks_.Clear();
		
		foreach(int iVisibleIndex in VisibleBlocks_) {
			// enable block
			if (ShouldDraw(iVisibleIndex)) {
				UseBlock(iVisibleIndex);
			}
			else {
				UnShownBlockList_.Enqueue(iVisibleIndex);
			}
		}
		
		// Clean up Used Queues
		CleanUpBlocks();
	}
	
	bool ShouldDraw(int iIndex) {
		int iX = 0, iY = 0, iZ = 0;
		GetCoordsOut(iIndex, out iX, out iY, out iZ);
		return !IsInRange(iX + 1, iY, iZ) || !GetBlock(CalcBlockIndex(iX + 1, iY, iZ)).bIsVisible
			|| !IsInRange(iX - 1, iY, iZ) || !GetBlock(CalcBlockIndex(iX - 1, iY, iZ)).bIsVisible
			|| !IsInRange(iX, iY + 1, iZ) || !GetBlock(CalcBlockIndex(iX, iY + 1, iZ)).bIsVisible
			|| !IsInRange(iX, iY - 1, iZ) || !GetBlock(CalcBlockIndex(iX, iY - 1, iZ)).bIsVisible
			|| !IsInRange(iX, iY, iZ + 1) || !GetBlock(CalcBlockIndex(iX, iY, iZ + 1)).bIsVisible
			|| !IsInRange(iX, iY, iZ - 1) || !GetBlock(CalcBlockIndex(iX, iY, iZ - 1)).bIsVisible;
	}
	
	void CleanUpBlocks() {
		/*if (iBlockQueueIndex_ == 1) {
			while (UseBlockList2_.Count > 0) {
				GameObject Block = UseBlockList2_.Dequeue();
				Block.renderer.enabled = false;
				UnusedBlockQueue_.Enqueue(Block);
			}
			iBlockQueueIndex_ = 2;
		}
		else if (iBlockQueueIndex_ == 2) {
			while (UseBlockList1_.Count > 0) {
				GameObject Block = UseBlockList1_.Dequeue();
				Block.renderer.enabled = false;
				UnusedBlockQueue_.Enqueue(Block);
			}
			iBlockQueueIndex_ = 1;
		}*/
		
		// clean up unshown blocks
		foreach(int iIndex in UnShownBlockList_) {
			SBlock BlockThing = GetBlock(iIndex);
			BlockThing.Reset();
			if (BlockModels_[iIndex] != null && BlockModels_[iIndex].GetComponent<Renderer>().enabled)
				BlockModels_[iIndex].GetComponent<Renderer>().enabled = false;
		}
		UnShownBlockList_.Clear();
	}
    private MaterialPropertyBlock materialPropBlock = new MaterialPropertyBlock();
    void UseBlock(int iIndex) {
		GameObject BlockObject;
		if (BlockModels_[iIndex] == null) {
			BlockObject = GameObject.Instantiate(BlockModel) as GameObject;

			BlockObject.transform.SetParent(this.transform);
			int iX = 0, iY = 0, iZ = 0;
			GetCoordsOut(iIndex, out iX, out iY, out iZ);
			BlockObject.transform.localPosition = GetPositionFromCoords(iX, iY, iZ);
            var blockColor = new Color(iX / (float)iXMax_, iY / (float)iYMax_, iZ / (float)iZMax_);
            materialPropBlock.SetColor("_Color", blockColor);
            BlockObject.GetComponent<MeshRenderer>().SetPropertyBlock(materialPropBlock);
			BlockModels_[iIndex] = BlockObject;
		}
		else
			BlockObject = BlockModels_[iIndex];
		BlockObject.GetComponent<Renderer>().enabled = true;


		/*GameObject BlockObject;
		if (iBlockQueueIndex_ == 1) {
			if (UseBlockList2_.Count > 0) {
				BlockObject = UseBlockList2_.Dequeue();
				UseBlockList1_.Enqueue(BlockObject);
			}
			else if (UnusedBlockQueue_.Count > 0) {
				BlockObject = UnusedBlockQueue_.Dequeue();
				UseBlockList1_.Enqueue(BlockObject);
			}
			else {
				BlockObject = GameObject.Instantiate(BlockModel) as GameObject;
				UseBlockList1_.Enqueue(BlockObject);
			}
		}
		else if (iBlockQueueIndex_ == 2) {
			if (UseBlockList1_.Count > 0) {
				BlockObject = UseBlockList1_.Dequeue();
				UseBlockList2_.Enqueue(BlockObject);
			}
			else if (UnusedBlockQueue_.Count > 0) {
				BlockObject = UnusedBlockQueue_.Dequeue();
				UseBlockList2_.Enqueue(BlockObject);
			}
			else {
				BlockObject = GameObject.Instantiate(BlockModel) as GameObject;
				UseBlockList2_.Enqueue(BlockObject);
			}
		}
		else  {
//			throw System.Exception("Bad");
			BlockObject = GameObject.Instantiate(BlockModel) as GameObject;
			UnusedBlockQueue_.Enqueue(BlockObject);
		}
		
		BlockObject.renderer.enabled = true;
		int iX = 0, iY = 0, iZ = 0;
		GetCoordsOut(iIndex, out iX, out iY, out iZ);
		BlockObject.transform.position = GetPositionFromCoords(iX, iY, iZ);
		BlockObject.renderer.material.color = new Color(iX / (float)iXMax_,
		                                                iY / (float)iYMax_,
		                                                iZ / (float)iZMax_);*/
	}
	
	void RunRule(int iIndex) {
		int iX = 0, iY = 0, iZ = 0;
		GetCoordsOut(iIndex, out iX, out iY, out iZ);

		// 1 1 1
		AddNeighbor(iX + 1, iY + 1, iZ + 1);
		// -1 -1 -1
		AddNeighbor(iX - 1, iY - 1, iZ - 1);

		// 1 1 0
		AddNeighbor(iX + 1, iY + 1, iZ);
		// 1 0 1
		AddNeighbor(iX + 1, iY, iZ + 1);
		// 0 1 1
		AddNeighbor(iX, iY + 1, iZ + 1);

		// -1 -1 0
		AddNeighbor(iX - 1, iY - 1, iZ);
		// -1 0 -1
		AddNeighbor(iX - 1, iY, iZ - 1);
		// 0 -1 -1
		AddNeighbor(iX, iY - 1, iZ - 1);

		// 1 1 -1
		AddNeighbor(iX + 1, iY + 1, iZ - 1);
		// 1 -1 1
		AddNeighbor(iX + 1, iY - 1, iZ + 1);
		// -1 1 1
		AddNeighbor(iX - 1, iY + 1, iZ + 1);

		// -1 -1 1
		AddNeighbor(iX - 1, iY - 1, iZ + 1);
		// -1 1 -1
		AddNeighbor(iX - 1, iY + 1, iZ - 1);
		// 1 -1 -1
		AddNeighbor(iX + 1, iY - 1, iZ - 1);

		// 0 1 -1
		AddNeighbor(iX, iY + 1, iZ - 1);
		// -1 0 1
		AddNeighbor(iX - 1, iY, iZ + 1);
		// 1 -1 0
		AddNeighbor(iX + 1, iY - 1, iZ);

		// -1 1 0
		AddNeighbor(iX - 1, iY + 1, iZ);
		// 1 0 -1
		AddNeighbor(iX + 1, iY, iZ - 1);
		// 0 -1 1
		AddNeighbor(iX, iY - 1, iZ + 1);


		// 0 0 -1
		AddNeighbor(iX, iY, iZ - 1);
		// 0 0 1
		AddNeighbor(iX, iY, iZ + 1);
		// 0 -1 0
		AddNeighbor(iX, iY - 1, iZ);
		// 0 1 0
		AddNeighbor(iX, iY + 1, iZ);
		// -1 0 0
		AddNeighbor(iX - 1, iY, iZ);
		// 1 0 0
		AddNeighbor(iX + 1, iY, iZ);
	}
	
	SBlock GetBlock(int iX, int iY, int iZ) {
		int iIndex = CalcBlockIndex(iX, iY, iZ);
		return GetBlock(iIndex);
	}
	
	SBlock GetBlock(int iIndex) {
		if (!IsInRange(iIndex))
			return null;

		//if (BlockMap_.TryGetValue(iIndex, out Block))
		if (BlockMap_[iIndex] != null)
			return BlockMap_[iIndex];

		SBlock Block;
		Block = new SBlock();
		
		//BlockMap_.Add(iIndex, Block);
		BlockMap_[iIndex] = Block;
		return Block;
	}
	
	int CalcBlockIndex(int iX, int iY, int iZ) {
		return iX + iY * iXMax_ + iZ * iXMax_ * iYMax_; 
	}
	
	void AddNeighbor(int iX, int iY, int iZ) {
		if (!IsInRange(iX, iY, iZ))
			return;
		int iIndex = CalcBlockIndex(iX, iY, iZ);
		SBlock NeighborBlock = GetBlock(iIndex);
		++NeighborBlock.iNeighbors;
		if (!NeighborBlock.bIsInUpdateList) {
			UpdateBlocks_.Add(iIndex);
			NeighborBlock.bIsInUpdateList = true;
		}
	}


	bool IsOn(bool bWasActive, int iNeighbors)
	{
		/*if (bWasActive)
			return iGrowthState_ < 0 ? (iNeighbors > 7 && iNeighbors < 15)
				: iGrowthState_ > 0 ? (iNeighbors > 3 && iNeighbors < 17)
				: (iNeighbors > 4 && iNeighbors < 16);
		// !bWasActive
		return iGrowthState_ < 0 ? iNeighbors > 11
			: iGrowthState_ > 0 ? iNeighbors > 8
			: iNeighbors > 10;*/
		if (bWasActive)
			return iGrowthState_ < 0 ? (iNeighbors > 7 && iNeighbors < 15)
				: iGrowthState_ > 0 ? (iNeighbors > 4 && iNeighbors < 16)
				: (iNeighbors > 4 && iNeighbors < 16);
		// !bWasActive
		return iGrowthState_ < 0 ? iNeighbors > 12
			: iGrowthState_ > 0 ? iNeighbors > 8
				: iNeighbors > 10;
	}
	
	bool IsInRange(int iIndex) {
		return iIndex >= 0 && iIndex < iMaxIndex_;
	}
	
	bool IsInRange(int iX, int iY, int iZ) {
		return iX >= 0 && iY >= 0 && iZ >= 0
			&& iX < iXMax_ && iY < iYMax_ && iZ < iZMax_;
	}
	
	void GetCoordsOut(int iIndex, out int iX, out int iY, out int iZ) {
		if (!IsInRange(iIndex)) {
			iX = -1;
			iY = -1;
			iZ = -1;
		}
		int iZMult = iXMax_ * iYMax_;
		iZ = iIndex / iZMult;
		int iYMult = iXMax_;
		iY = (iIndex - iZ * iZMult) / iYMult;
		int iXMult = 1;
		iX = (iIndex - iY * iYMult - iZ * iZMult) / iXMult;
		if (!IsInRange(iX, iY, iZ)) {
			iX = -1;
			iY = -1;
			iZ = -1;
		}
	}
	
	
	int GetIndexFromPosition(Vector3 Position) {
		Position /= fBlockSize_;
		
		int iZ = Mathf.RoundToInt(Position[2]);
		int iY = Mathf.RoundToInt(Position[1]);
		int iX = Mathf.RoundToInt(Position[0]);
		
		if (!IsInRange(iX, iY, iZ)) {
			return -1;
		}
		return CalcBlockIndex(iX, iY, iZ);
	}
	
	Vector3 GetPositionFromCoords(int iX, int iY, int iZ) {
		return new Vector3(iX * fBlockSize_, iY * fBlockSize_, iZ * fBlockSize_);
	}
	
	
	
	/*void OnGUI() {
		GUI.TextField(new Rect(0, 0, 100, 30), "Dimensions");
		
		sXMax_ = GUI.TextField(new Rect(0, 30, 100, 30), sXMax_);
		sYMax_ = GUI.TextField(new Rect(0, 60, 100, 30), sYMax_);
		sZMax_ = GUI.TextField(new Rect(0, 90, 100, 30), sZMax_);
		
		
		if (GUI.Button(new Rect(0, 120, 100, 30), "Reset")) {
			int.TryParse(sXMax_, out iXMax_);
			int.TryParse(sYMax_, out iYMax_);
			int.TryParse(sZMax_, out iZMax_);
			ResetGame();
		}
	}*/
}
