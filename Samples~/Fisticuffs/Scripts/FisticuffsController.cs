using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FisticuffsController : MonoBehaviour {
	public static FisticuffsController Instance { get; private set; }

	public int maxNumberOfCardsInPlay = 2;
	public int cardsNeededForGameToStart = 2;
	public GameObject bell;
	public GameObject megaHit;
	public GameObject megaHitParticles;
	public AudioClip pop;
	public AudioClip punchHit;
	public AudioClip punchMiss;
	public AudioClip tada;
	public AudioClip crash;
	public AudioClip victory;
	public AudioClip hitExplosion;
	public List<GameObject> cardsInPlay = new List<GameObject>();
	private bool ready = false;
	public float timeAllowedBeforeReset = 5;
	public AudioSource oneShotAudio;
	public AudioSource crowdAudio;
	public float megaDamageAmount = 20;
	
	public bool gameHasStarted = false;
	public bool gameIsDone = false;
	private Transform character1;
	private Transform character2;
	private CharacterBehaviors char1behaviors;
	private CharacterBehaviors char2behaviors;

	void Awake() {
		if (null == Instance) {
			Instance = this;
		} else {
			Debug.LogError("FisticuffsController::Awake - Instance already set. Is there more than one in scene?");
		}
	}

	void Start () {
		bell.SetActive(false);
		megaHit.SetActive(false);
	}
	
	void Update () {
		CheckIfTwoCards();
		if (ready == true) {
			LookAtEachOther();
		}
	}
	
	// checks if two cards are in view and stores references to their associated character scripts
	// if there are two. If not two cards, reset certain booleans, etc.
	void CheckIfTwoCards() {

		if (cardsInPlay.Count == cardsNeededForGameToStart && ready == false) {
			character1 = cardsInPlay[0].transform;//.transform.FindChild("Character");
			character2 = cardsInPlay[1].transform;//.transform.FindChild("Character");
			char1behaviors = character1.GetComponent<CharacterBehaviors>();
			char2behaviors = character2.GetComponent<CharacterBehaviors>();
			char1behaviors.myPositionInControllerList = 0;
			char2behaviors.myPositionInControllerList = 1;
			ready = true;
			StartCoroutine(IntroStart());
		}
		if (cardsInPlay.Count != cardsNeededForGameToStart && ready == true) {
			ready = false;
			gameHasStarted = false;
			bell.GetComponent<Animation>().Stop();
			bell.SetActive(false);
			crowdAudio.Stop();
		}
	}
	
	IEnumerator IntroStart() {
		TakeAim();
		yield return new WaitForSeconds(1.7f);
		if (ready == true) {
			bell.SetActive(true);
			bell.GetComponent<Animation>().Play("BellStart");
			// turn off attributes windows when fight is about to start
			char1behaviors.myAttributes.SetActive(false);
			char2behaviors.myAttributes.SetActive(false);
		}
		
	}
	
	public void GameStart() {
		crowdAudio.Play();
		gameHasStarted = true;
		char1behaviors.myHealthHolder.SetActive(true);
		char2behaviors.myHealthHolder.SetActive(true);
	}
	
	// they track and face eachother if not being animated otherwise
	void LookAtEachOther() {
	
		// tracking error sometimes results in two characters being placed on the same marker
		// in these cases, there is no clean concept of "looking at each other" so - do nothing
		if (Vector3.Distance(character1.position, character2.position) > 0.01) {

			if (char1behaviors.okToAnimate == true) {
				Quaternion rotation1 = Quaternion.LookRotation(character2.position - character1.parent.transform.position, character1.parent.transform.up); 
				character1.transform.rotation = Quaternion.Slerp(character1.transform.rotation, rotation1, Time.deltaTime * 2);
			}
			
			if (char2behaviors.okToAnimate == true) {
				Quaternion rotation2 = Quaternion.LookRotation(character1.position - character2.parent.transform.position, character2.parent.transform.up);
				character2.transform.rotation = Quaternion.Slerp(character2.transform.rotation, rotation2, Time.deltaTime * 2);
			}
		}
	}
	
	// lock onto opponents target point as a place where all future target colliders will be
	// instantiated during this fight
	void TakeAim() {
		if (char1behaviors.opponentTargetPoint != character2.Find("TargetPoint")) {
			char1behaviors.opponentTargetPoint = character2.Find("TargetPoint"); 
		}
		if (char2behaviors.opponentTargetPoint != character1.Find("TargetPoint")) {
			char2behaviors.opponentTargetPoint = character1.Find("TargetPoint"); 
		}
	}
	
	
	public void GameEnd() {
		oneShotAudio.PlayOneShot(crash);
		StartCoroutine(EndBell());
	}
	
	IEnumerator EndBell() {
		yield return new WaitForSeconds(1.0f);
		bell.SetActive(true);
		bell.GetComponent<Animation>().Play("BellStop");
		oneShotAudio.PlayOneShot(victory);
	}
}
