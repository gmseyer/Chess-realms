using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputFieldGrabber : MonoBehaviour
{
    [Header("The value we got from the input field")]
	[SerializeField] private string inputText;
	
	[Header("Showing the reaction to player")]
	[SerializeField] private GameObject reactionGroup;
	[SerializeField] private TMP_Text reactionTextBox;
	
	public void GrabFromInputField (string input)
	{
		inputText = input;
		DisplayReactionToInput();
		
		
	}
	
	private void DisplayReactionToInput(){
	reactionTextBox.text = "Welcome to the team, " + inputText;
	reactionGroup.SetActive(true);
	
	}
}
