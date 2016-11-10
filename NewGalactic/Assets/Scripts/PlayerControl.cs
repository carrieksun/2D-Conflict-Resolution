﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using ExitGames.Demos.DemoAnimator;

public class PlayerControl : Photon.PunBehaviour
{
	[HideInInspector]
	public bool facingRight = true;			// For determining which way the player is currently facing.
	[HideInInspector]
	public bool jump = false;				// Condition for whether the player should jump.
	private bool doublejump = false;

	public float moveForce = 365f;			// Amount of force added to move the player left and right.
	public float maxSpeed = 5f;				// The fastest the player can travel in the x axis.
	public AudioClip[] jumpClips;			// Array of clips for when the player jumps.
	public float jumpForce = 1000f;			// Amount of force added when the player jumps.		// Delay for when the taunt should happen.

	private Transform groundCheck;			// A position marking where to check if the player is grounded.
	private bool grounded = false;			// Whether or not the player is grounded.
	public Animator anim;					// Reference to the player's animator component.

	public static bool dontScroll = false;

	[Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
	public static GameObject LocalPlayerInstance =null;
	public static GameObject NonLocalPlayerInstance = null;

	public bool thisOneIsLocal = false;

	void Awake()
	{

		if (GetComponent<PhotonView> ().owner.isLocal) {
			if (PlayerControl.LocalPlayerInstance == null) {
				PlayerControl.LocalPlayerInstance = this.gameObject;
				ApplyCharacterScript.otherPlayerIsReadyToNextLevel = false;
				ApplyCharacterScript.isReadyToNextLevel = false;
				GetComponentInChildren<SpriteRenderer> ().sortingOrder = 1; 
				thisOneIsLocal = true;
			} else {
				Destroy (gameObject);
			}
		} else {
			if (PlayerControl.NonLocalPlayerInstance == null) {
				PlayerControl.NonLocalPlayerInstance = this.gameObject;
				ApplyCharacterScript.otherPlayerIsReadyToNextLevel = false;
				ApplyCharacterScript.isReadyToNextLevel = false;
				thisOneIsLocal = false;

			} else {
				Destroy (gameObject);
			}
		}

		// #Critical
		// we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
		//DontDestroyOnLoad(this.gameObject);

		// Setting up references.
		groundCheck = transform.Find("groundCheck");
	}

	void Start()
	{
		if (GetComponent<PhotonView> ().owner.isLocal) {

			CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork> ();

			if (_cameraWork != null) {
				if (GetComponent<PhotonView> ().owner.isLocal) {
					_cameraWork.OnStartFollowing ();
				}
			} else {
				Debug.LogError ("<Color=Red>Missing</Color> CameraWork Component on playerPrefab.", this);
			}
		}
	}

	void Update()
	{
		if (!ApplyCharacterScript.isReadyToNextLevel) {
			if (GetComponent<PhotonView> ().owner.isLocal) {

				// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
				grounded = Physics2D.Linecast (transform.position, groundCheck.position, 1 << LayerMask.NameToLayer ("Ground"));  

				// If the jump button is pressed and the player is grounded then the player should jump.
				if (Input.GetButtonDown ("Jump") && grounded) {
					jump = true;
					doublejump = false;
				} 
				if (Input.GetButtonDown ("Jump") && !jump && !doublejump) {
					jump = true;
					doublejump = true;
				}
			}
		} else {
			GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
		}
	}
	void FixedUpdate ()
	{
		if (!ApplyCharacterScript.isReadyToNextLevel) {
			if (GetComponent<PhotonView> ().owner.isLocal) {

				// Cache the horizontal input.
				float h = Input.GetAxis ("Horizontal");
				if (grounded) {
					// The Speed animator parameter is set to the absolute value of the horizontal input.
					if (h < 0f) {
						anim.SetFloat ("Speed", Mathf.Abs (-h));
					} else {
						anim.SetFloat ("Speed", Mathf.Abs (h));
					}
				} else {
					anim.SetFloat ("Speed", 0f);

				}
				// If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
				if (h * GetComponent<Rigidbody2D> ().velocity.x < maxSpeed)
					// ... add a force to the player.
					GetComponent<Rigidbody2D> ().AddForce (Vector2.right * h * moveForce);

				// If the player's horizontal velocity is greater than the maxSpeed...
				if (Mathf.Abs (GetComponent<Rigidbody2D> ().velocity.x) > maxSpeed)
					// ... set the player's velocity to the maxSpeed in the x axis.
					GetComponent<Rigidbody2D> ().velocity = new Vector2 (Mathf.Sign (GetComponent<Rigidbody2D> ().velocity.x) * maxSpeed, GetComponent<Rigidbody2D> ().velocity.y);

				// If the input is moving the player right and the player is facing left...
				if (h > 0 && !facingRight)
					// ... flip the player.
					Flip ();
				// Otherwise if the input is moving the player left and the player is facing right...
				else if (h < 0 && facingRight)
					// ... flip the player.
					Flip ();

				// If the player should jump...
				if (jump) {
					if (doublejump) {
						Debug.Log ("Should double jump");
						GetComponent<Rigidbody2D> ().velocity = (new Vector2 (GetComponent<Rigidbody2D> ().velocity.x, 0f));
						GetComponent<Rigidbody2D> ().AddForce (new Vector2 (0f, jumpForce * .8f));
						// Make sure the player can't jump again until the jump conditions from Update are satisfied.
						jump = false;
						//doublejump = false;
					} else {

						// Set the Jump animator trigger parameter.
						//anim.SetTrigger("Jump");

						// Play a random jump audio clip.
						//int i = Random.Range(0, jumpClips.Length);
						//AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);
						// Add a vertical force to the player.
						Debug.Log ("Reg jump");
						GetComponent<Rigidbody2D> ().AddForce (new Vector2 (0f, jumpForce));

						// Make sure the player can't jump again until the jump conditions from Update are satisfied.
						jump = false;
					}
				}
			}
		} else {
			GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
		}
	}


	void Flip ()
	{
		if (GetComponent<PhotonView> ().owner.isLocal) {

			// Switch the way the player is labelled as facing.
			facingRight = !facingRight;

			// Multiply the player's x local scale by -1.
			Vector3 theScale = transform.localScale;
			theScale.x *= -1;
			transform.localScale = theScale;
		}
	}

	void OnTriggerEnter2D(Collider2D other){
		if (GetComponent<PhotonView> ().owner.isLocal) {

			if (other.gameObject.CompareTag ("NoScrollZone")) {
				dontScroll = true;
			}
		}
	}

	void OnTriggerExit2D(Collider2D other){
		if (GetComponent<PhotonView> ().owner.isLocal) {

			if (other.gameObject.CompareTag ("NoScrollZone")) {
				dontScroll = false;
			}
		}
	}
}
