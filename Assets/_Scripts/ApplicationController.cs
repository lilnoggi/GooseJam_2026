using UnityEngine;

/// <summary>
/// A universal script to handle global application states
/// Can be attatched to Main Menu, Pause Menu, or Game Over screens
/// </summary>
public class ApplicationController : MonoBehaviour
{
    /// <summary>
    /// Closes the application. 
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
