using UnityEngine;
using System.Collections;

public class CharacterBehaviors : MonoBehaviour {
	private enum PopupMode { None = 0, Started = 1, InProgress = 2, Ending = 3 };

	private static Camera arCamera = null;
	private float originalTouchPosY;

	private float popTargetHeight;
	private PopupMode popUpMode = PopupMode.None;

	// set to true when character is being touched on screen
	private bool imTouched;
	private bool okToPlaySounds = false;

	private bool iLost = false;

	// Gloves Punching
	private float punchTimer;
	private float punchStartTime;
	private float maxPunchTime = .65f;
	private bool punchTimerHasStarted = false;
	private bool glovesShouldRetract = true;
	private Transform punchingGlove;
	private Vector3 punchingGloveStartPosition;

	// Health
	private float healthScaleFactor = 0;
	private Vector3 healthOriginalScale;
	private float totalHealthPoints = 100.0f;

	// Face
	private GameObject myFace;
	
	
	public Transform leftShoulderAttachPoint;
	public Transform rightShoulderAttachPoint;
	public Transform leftGloveAttachPoint;
	public Transform rightGloveAttachPoint;
	
	public int myPositionInControllerList;
	public GameObject myAttributes;
	public GameObject myHealthHolder;
	public GameObject myHealthScaler;
	public GameObject targetPointColliderObj; // prefab that gets instantiated as a collider target for fist that is punching
	public GameObject myTempTarget; // where the instantiated prefab is stored for later reference (for destroying etc)
	public Transform opponentTargetPoint; // place where the collider prefab gets instantiated (near opponent's face)
	public GameObject myLeftGloveHolder;
	public GameObject myRightGloveHolder;
	public Transform leftGlove;
	public Transform rightGlove;
	public Vector3 origLeftGloveLocalPos;
	public Vector3 origRightGloveLocalPos;
	public float startLocalPosY = -100;
	public int punchPhase = 0; // 0 = not punching, 1 = fist moving toward target, 2 = fist moving back to orig position
	public bool okToAnimate; // can I punch, hover, etc? Disallowed during pop-up and during touches
	
	// attributes for this character:
	public float damageGivenPerPunch = 5.0f; // punch power
	public float attackTimeIncrement = 1.25f; // punch frequency : lower increment = less time between punches
	public float punchSpeedModifier = .05f; // punch speed : lower number is faster, but above .1 is pretty slow
	public float defenseModifier = .5f; // defensive strength : should be bettween 0 & 1; Higher is better, with 0 providing no defense and 1 providing perfect defense (no damage will be possible)
	public int chanceOfMassiveHit = 5; // probablity out of 1000 to score large damage with a single hit
	private float modifiedAttackTimeIncrement = 0; // all characters have their attack time modified by a random amount each time they punch

	private bool allowTouch = true; // start allowing touch. but if holding down while losing fight, disable touch
	private bool loseAnimPlayed = false; // added to keep us from replaying losing animation after we lost already
	
	private Vector2 lastTouchPos;
	private bool lastTouchValid = false;

	private Animation characterAnimation = null;
	private Animation faceAnimation = null;

	void Awake() {
		characterAnimation = transform.parent.gameObject.GetComponent<Animation>();
		if (null == characterAnimation) {
			characterAnimation = transform.parent.gameObject.AddComponent<Animation>();
		}
	}

	void Start() {
		if (null == CharacterBehaviors.arCamera) {
			CharacterBehaviors.arCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
		}

		Transform myFaceTransform = transform.Find("Face");
		myFace = myFaceTransform.gameObject;

		faceAnimation = myFace.GetComponent<Animation>();
		if (null == faceAnimation) {
			faceAnimation = myFace.AddComponent<Animation>();
		}

		// Punching was really slow on Android!
		if (Application.platform == RuntimePlatform.Android) {
			punchSpeedModifier *= 0.5f;
		}
	}
	
	void OnEnable() {
		ResetMe();
		okToAnimate = false;

		// First thing that happens when card is in view - character pops up.
		StartPopUp();

		if (FisticuffsController.Instance.cardsInPlay.Count < FisticuffsController.Instance.maxNumberOfCardsInPlay
		    && !FisticuffsController.Instance.cardsInPlay.Contains(gameObject)) {
			FisticuffsController.Instance.cardsInPlay.Add(gameObject);
		}
	}

	void OnDisable() {
		if (null == FisticuffsController.Instance) {
			Debug.LogError("CharacterHolder::OnDisable - FisticuffsController.Instance not set. Is there one in the scene?");
			return;
		}
		
		FisticuffsController.Instance.cardsInPlay.Remove(gameObject);
	}
	
	void ResetMe() {
		if (null == FisticuffsController.Instance) {
			Debug.LogError("CharacterBehaviors::ResetMe - FisticuffsController.Instance not set. Is there one in the scene?");
			return;
		}

		characterAnimation.Play("CharacterReset");
		SetHealthScaleFactor();
		SetHealth(100);
		myAttributes.SetActive(true);
		myHealthHolder.SetActive(false);
		leftGlove.localPosition = origLeftGloveLocalPos;
		rightGlove.localPosition = origRightGloveLocalPos;
		punchTimerHasStarted = false;
		glovesShouldRetract = true;
		FisticuffsController.Instance.gameIsDone = false;
		okToAnimate = true;
		iLost = false;
		punchPhase = 0;
		allowTouch = true; // reset here after we lose or win.
		loseAnimPlayed = false; // reset for new game.
		gameObject.GetComponent<Collider>().enabled = true; // disable touch input for me till resetting game!
	}
	
	void Update () {
		CheckForGameOver();
		TouchTracker();
		GlovesRetract();
		PopUp();
		SetUpPunch();
		Punch();
		Hover();
		ResetCheck();
	}
		
	void TouchTracker() {
		if (null == FisticuffsController.Instance) {
			Debug.LogError("CharacterBehaviors::TouchTracker - FisticuffsController.Instance not set. Is there one in the scene?");
			return;
		}

		Vector2 touchPos = Vector2.zero;
		Vector2 touchDeltaPos = Vector2.zero;
		bool touchActive;
		bool touchBegan = false;
		bool touchMoved = false;
		bool touchEnded = false;

		// Default to touch input, otherwise use mouse input.
		if (Input.touchSupported) {
			touchActive = Input.touchCount > 0;
			if (touchActive) {
				touchPos = Input.GetTouch(0).position;
				touchDeltaPos = Input.GetTouch(0).deltaPosition;
				touchBegan = (Input.GetTouch(0).phase == TouchPhase.Began);
				touchMoved = (Input.GetTouch(0).phase == TouchPhase.Moved);
				touchEnded = (Input.GetTouch(0).phase == TouchPhase.Ended);
			}
		} else {
			touchActive = Input.GetMouseButton(0) || Input.GetMouseButtonUp(0);
			if (touchActive) {
				touchPos = Input.mousePosition;
				if (lastTouchValid) {
					touchDeltaPos = touchPos - lastTouchPos;
				}
				else {
					touchDeltaPos = Vector2.zero;
				}

				touchBegan = Input.GetMouseButtonDown(0);
				touchEnded = Input.GetMouseButtonUp(0);
				touchMoved = !touchBegan && !touchEnded;
			}
		}

		if (touchActive && allowTouch && !loseAnimPlayed) {

			// Added to keep from touches if we lost the game.
			if(iLost) {
				imTouched = false;
				okToAnimate = true;
				allowTouch = false;
			}

			Ray ray;
			RaycastHit hit;
			ray = arCamera.ScreenPointToRay (touchPos);
			
			if (FisticuffsController.Instance.gameIsDone == false) {
				if (Physics.Raycast (ray, out hit, 1000)) {

					if(touchBegan && hit.collider.gameObject == gameObject){
						imTouched = true;
						okToAnimate = false;
						originalTouchPosY = touchPos.y;
					}
				}
			
				if (touchMoved && imTouched == true && touchPos.y < originalTouchPosY) { 
					Vector3 newPosition = new Vector3(0, transform.localPosition.y + (touchDeltaPos.y) * .8f, 6); // .8 was .2
					if (newPosition.y < startLocalPosY) {
						newPosition.y = startLocalPosY;
					}
					transform.localPosition = newPosition;
				}
			
				if (touchEnded && imTouched == true) { 
					imTouched = false;
					if (!Mathf.Approximately(transform.localPosition.y, 30)) {
						StartPopUp();
					} else {
						StopPopUp();
					}
				}
			}
		}

		lastTouchValid = touchActive;
		lastTouchPos = touchPos;
	}

	
	// Lerps to the various y positions needed for the procedural pop up animation:
	void PopUp() { 
		if(iLost) {
			return;
		}

		Vector3 myPos = transform.localPosition;

		if (popUpMode == PopupMode.Started && myPos.y < 50) {
			popTargetHeight = 50;
		} else if (popUpMode == PopupMode.Started && (Mathf.Approximately(myPos.y, 50) || myPos.y > 50)) {
			popUpMode = PopupMode.InProgress;
		}
			
		if (popUpMode == PopupMode.InProgress && myPos.y > 15) {
			popTargetHeight = 15;
		} else if (popUpMode == PopupMode.InProgress && (Mathf.Approximately(myPos.y, 15) || myPos.y < 15)) {
			popUpMode = PopupMode.Ending;
		}
			
			
		if (popUpMode == PopupMode.Ending && myPos.y < 30) {
			popTargetHeight = 30;
		} else if (popUpMode == PopupMode.Ending && (Mathf.Approximately(myPos.y, 30) || myPos.y > 30)) {
			StopPopUp();
		}
		
		if (popUpMode != PopupMode.None) {
			float newY = Mathf.Lerp(myPos.y, popTargetHeight, 1);
			Vector3 newPos = new Vector3(0, newY, 6);
			transform.localPosition = newPos;
		}
		
	}
	
	void StartPopUp() {
		if (null == FisticuffsController.Instance) {
			Debug.LogError("CharacterBehaviors::StartPopUp - FisticuffsController.Instance not set. Is there one in the scene?");
			return;
		}

		popUpMode = PopupMode.Started;
	
		if (okToPlaySounds == true) { // don't let the sound play as app is first setting up (first game loop on enable)
			FisticuffsController.Instance.oneShotAudio.PlayOneShot(FisticuffsController.Instance.pop);
		}
		okToPlaySounds = true;
	}
	
	void StopPopUp() {
		popUpMode = PopupMode.None;
		okToAnimate = true;
	}
	
	// Controls the movement of the character's hands
	// as he moves into and out of his hole, e.g. they move above his head as he decends:
	void GlovesRetract() { 
		if (glovesShouldRetract == true) {
			float newZ = (transform.localPosition.y - 30) * 0.69230769f;
			Vector3 leftAngles = myLeftGloveHolder.transform.localEulerAngles;
			myLeftGloveHolder.transform.localEulerAngles = new Vector3(leftAngles.x, leftAngles.y, newZ);
			
			Vector3 rightAngles = myRightGloveHolder.transform.localEulerAngles;
			myRightGloveHolder.transform.localEulerAngles = new Vector3(rightAngles.x, rightAngles.y, -newZ);
		}
	}
	
	// A timer controls the frequency of the character's punches, then sets a target,
	// and randomly chooses right or left hand to punch. The target object is instantiated
	// and parented to the character who is punching, because we don't want the fist to follow
	// the opponent as he dodges. The instantiated target then acts as a collider to detect
	// when the punch has missed the opponent but reached it's destination.
	void SetUpPunch() {
		if (null == FisticuffsController.Instance) {
			Debug.LogError("CharacterBehaviors::SetUpPunch - FisticuffsController.Instance not set. Is there one in the scene?");
			return;
		}

		if (okToAnimate == true && FisticuffsController.Instance.gameIsDone == false) {
			if (FisticuffsController.Instance.gameHasStarted == true && punchTimerHasStarted == false){
				punchTimerHasStarted = true;
				punchTimer = Time.time;
			} 
			if (FisticuffsController.Instance.gameHasStarted == false) {
				punchTimerHasStarted = false;
			}
		
			if (FisticuffsController.Instance.gameHasStarted == true) {
				if (Time.time > punchTimer + modifiedAttackTimeIncrement) {
					punchTimer = Time.time;
					GameObject targetCollider = Instantiate(targetPointColliderObj, opponentTargetPoint.position, Quaternion.identity) as GameObject;
					targetCollider.tag = GloveScript.TARGET_TAG;
					targetCollider.transform.parent = transform;
					myTempTarget = targetCollider;
					punchPhase = 1;
					punchStartTime = Time.time;
					int whichHand = Random.Range(1,3); // right or left handed punch selected randomly
					if (whichHand == 1){
						punchingGlove = leftGlove;
						punchingGloveStartPosition = origLeftGloveLocalPos;
					} else {
						punchingGlove = rightGlove;
						punchingGloveStartPosition = origRightGloveLocalPos;
					}
					modifiedAttackTimeIncrement = attackTimeIncrement + Random.Range(-.5f , .5f);					
				}
			}
		}
	}
	
	// Controls the two punch phases, toward opponent and coming back to self, plus zero phase if no punch is happening:
	void Punch() { 
		// if an arm gets stuck in an extended position for some reason (loss of tracking, etc.) the timer will retract it:
		if (punchPhase == 1 && Time.time > punchStartTime + maxPunchTime) { 
			punchPhase = 2;
		}
		
		Vector3 velocity = Vector3.zero; // current velocity for SmoothDamp function
		
		// fist on its way toward target:
		if (punchPhase == 1) { 
			if(punchingGlove.position != myTempTarget.transform.position){
				punchingGlove.position = Vector3.SmoothDamp(punchingGlove.position, myTempTarget.transform.position, ref velocity, punchSpeedModifier);
			}
		}
			
		// fist on its way back from target:
		if (punchPhase == 2) { 
			if(!Mathf.Approximately(punchingGlove.localPosition.x, punchingGloveStartPosition.x) || !Mathf.Approximately(punchingGlove.localPosition.z, punchingGloveStartPosition.z)){
				punchingGlove.localPosition = Vector3.SmoothDamp(punchingGlove.localPosition, punchingGloveStartPosition, ref velocity, punchSpeedModifier);
			} else {
				punchPhase = 0;
			}
		}
		
	}
	
	// starts/stops the hovering up/down idle animation:
	void Hover() { 
		if (okToAnimate == true) {
			if (!characterAnimation.IsPlaying("CharacterHover")){
				characterAnimation.Play("CharacterHover");
			}
			if (!faceAnimation.IsPlaying("FaceHit")){
				faceAnimation.Play("FaceIdle");
			}
		} else {
			if (characterAnimation.IsPlaying("CharacterHover")) {
				characterAnimation.Stop();
			}
		}
	}
	
	// reactivates attributes floating window if the other character disappears
	void ResetCheck() { 
		if (null == FisticuffsController.Instance) {
			Debug.LogError("CharacterBehaviors::ResetCheck - FisticuffsController.Instance not set. Is there one in the scene?");
			return;
		}

		if (myHealthHolder.activeInHierarchy == true && FisticuffsController.Instance.cardsInPlay.Count != FisticuffsController.Instance.cardsNeededForGameToStart) {
			ResetMe();
		}
	}
	
	// uses individual character's attributes variables combined with random slight modification to
	// come up with damage per blow. Also does a random check for mega hit based on the particular
	// character's chances/attribute of that happening.
	public void CalculateDamageToOpponent() {
		if (null == FisticuffsController.Instance) {
			Debug.LogError("CharacterBehaviors::CalculateDamageToOpponent - FisticuffsController.Instance not set. Is there one in the scene?");
			return;
		}

		int myOpponentListNum = 0;
		if (myPositionInControllerList == 0) { // if I am position 0, opponent is position 1, otherwise it's vice versa and myOpponentListNum remains 0
			myOpponentListNum = 1;
		}

		Transform opponentCharacter = FisticuffsController.Instance.cardsInPlay[myOpponentListNum].transform;
		CharacterBehaviors opponentBehaviors = opponentCharacter.gameObject.GetComponent<CharacterBehaviors>();
		
		float randomDamageModifier = Random.Range(-1.0f, 1.0f);
		float randomDefenseModifier = Random.Range(-.1f, .1f);
		
		float damageBeforeDefense = (damageGivenPerPunch + randomDamageModifier);
		float damageCalc = damageBeforeDefense - (opponentBehaviors.defenseModifier * damageBeforeDefense);

		float damageFinal;
		int rollForMassiveHit = Random.Range(0, 1001);
		if (rollForMassiveHit <= chanceOfMassiveHit) {
			damageFinal = FisticuffsController.Instance.megaDamageAmount;
		} else {	
			damageFinal = damageCalc;
		}
		
		opponentBehaviors.ReceiveDamage(damageFinal);
	}

	// subtracts damage points from current health and checks for zero (game over)
	public void ReceiveDamage(float howMuchDamage) { 
		if (null == FisticuffsController.Instance) {
			Debug.LogError("CharacterBehaviors::ReceiveDamage - FisticuffsController.Instance not set. Is there one in the scene?");
			return;
		}
		
		faceAnimation.Play("FaceHit");
		
		// was it a mega hit? if so, do the explosion stuff:
		if (howMuchDamage == FisticuffsController.Instance.megaDamageAmount) {
			Vector3 megaParticlesPosition = new Vector3(transform.position.x, transform.position.y + 50, transform.position.z);
			FisticuffsController.Instance.megaHit.SetActive(true);
			FisticuffsController.Instance.oneShotAudio.PlayOneShot(FisticuffsController.Instance.hitExplosion);
			GameObject megaParticles = Instantiate(FisticuffsController.Instance.megaHitParticles, megaParticlesPosition, Quaternion.identity) as GameObject;
		}
		

		float newHealthPoints = totalHealthPoints - howMuchDamage;
		if (newHealthPoints <= 0) {
			newHealthPoints = 0;
			FisticuffsController.Instance.oneShotAudio.PlayOneShot(FisticuffsController.Instance.hitExplosion);
			FisticuffsController.Instance.gameIsDone = true;
			iLost = true;
		}
		SetHealth(newHealthPoints);
	}
	
	// scales the health bar display based on float input
	void SetHealth(float newHealthPoints) { 
		totalHealthPoints = newHealthPoints;
		float newHealthScaleX = totalHealthPoints / healthScaleFactor;
		myHealthScaler.transform.localScale = new Vector3(newHealthScaleX, healthOriginalScale.y, healthOriginalScale.z);
	}
	
	// runs once at start of game to set up the math by which we scale the health bar
	void SetHealthScaleFactor() {  
		if (healthScaleFactor == 0) { 
			healthOriginalScale = myHealthScaler.transform.localScale;
			healthScaleFactor = (100 / healthOriginalScale.x);	
		}
	}
	
	void CheckForGameOver() {
		if (null == FisticuffsController.Instance) {
			Debug.LogError("CharacterBehaviors::ReceiveDamage - FisticuffsController.Instance not set. Is there one in the scene?");
			return;
		}

		if (FisticuffsController.Instance.gameIsDone == true && okToAnimate == true && !loseAnimPlayed) { // added loseAnimPlayed ---<<
			okToAnimate = false;
			transform.localPosition = new Vector3(0, 30, 6);
				
			if (iLost == true) {
				this.gameObject.GetComponent<Collider>().enabled = false; // disable touch input for me till resetting game!
				loseAnimPlayed = true;
				FisticuffsController.Instance.GameEnd();
				characterAnimation.Play("CharacterLose");
			} else {
				characterAnimation.Play("CharacterWin");
			}
		}	
	}
	
	
}
