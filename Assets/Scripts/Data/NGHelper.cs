using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NGHelper : MonoBehaviour
{
    public io.newgrounds.core ngio_core;

    void Start()
    {
        ngio_core.onReady(() => {

          // Call the server to check login status
          ngio_core.checkLogin((bool logged_in) => {

            if(logged_in)
            {
                onLoggedIn();
            }
            else
            {
                // Opens up Newgrounds Passport if they are not logged in
                requestLogin();
            }
          });
        });
    }

    // Gets called when the Player is signed in
    void onLoggedIn()
    {
        // Do something. You can access the player's info with:
        io.newgrounds.objects.user player = ngio_core.current_user;
    }

    // When the user clicks the Log In button
    void requestLogin()
    {
        // This opens passport and tells the core what to do when a difinitive result comes back.
        ngio_core.requestLogin(onLoggedIn, onLoginFailed, onLoginCancelled);
    }

    void onLoginFailed()
    {
        // Do something. You can access the login error with:
        io.newgrounds.objects.error error = ngio_core.login_error;
    }

    void onLoginCancelled()
    {
        // Do something...
    }

    public void unlockMedal(int medal_id)
    {
        // Create the component.
        io.newgrounds.components.Medal.unlock medal_unlock = new io.newgrounds.components.Medal.unlock();

        // Set required parameters.
        medal_unlock.id = medal_id;

        // Call the component on the server and tell it to fire onMedalUnlock when it's done.
        medal_unlock.callWith(ngio_core);
        Debug.Log("Sent a message to the server to unlock a medal.");
    }

    public void NGSubmitScore(int score_id, int score)
    {
        io.newgrounds.components.ScoreBoard.postScore submit_score = new io.newgrounds.components.ScoreBoard.postScore();
        submit_score.id = score_id;
        submit_score.value = score;
        submit_score.callWith(ngio_core);
        Debug.Log("Sent a message to the server to submit the score to the leaderboard.");
    }
}
