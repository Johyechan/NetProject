using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthManager : Singleton<AuthManager>
{
    [Header("Firebase")]
    public FirebaseAuth auth; //인증 관리 객체
    public FirebaseUser User; //사용자
    public DatabaseReference DBref; //데이터베이스 인스턴스

    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;

    [Header("Register")]
    public TMP_InputField emailRegisterField;
    public TMP_InputField userNameRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordCheckRegisterField;
    public TMP_Text warningRegisterText;

    [Header("ChangePassword")]
    public TMP_InputField currentEmailRegisterField;
    public TMP_InputField currentPasswordRegisterField;
    public TMP_InputField changePasswordCheckRegisterField;
    public TMP_Text warningPasswordText;

    [Header("ShowFriends")]
    public Button buttonprefab;
    public Transform buttonParent;
    public TMP_Text friends;

    public Material enemyMaterial;
    public TMP_Text loginCountText;
    public TMP_Text userNameText;

    private string strLastLogin;
    private string enemycolor;
    private int loginCount;
    private bool newfriends = false;

    private List<string> userIds = new List<string>();
    private List<string> friendsList = new List<string>();

    private new void Awake()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                auth = FirebaseAuth.DefaultInstance;
                DBref = FirebaseDatabase.DefaultInstance.RootReference;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    private void Update()
    {
        // 친구 누구 있는지 보여주기
        if(newfriends)
        {
            friends.text += friendsList[friendsList.Count - 1] + '\n';
            newfriends = false;
        }
        if(GameManager.Instance.powerUp || UIManager.Instance.changeColor)
        {
            StartCoroutine(ChangeColor());
        }
    }

    private IEnumerator Register(string email, string password, string userName)
    {
        if (userName == "")
        {
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordCheckRegisterField.text)
        {
            warningRegisterText.text = "Password does not Match!";
        }
        else
        {
            var task = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to Register:{task.Exception}");
                FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
                AuthError errorcode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorcode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                warningRegisterText.text = message;

            }
            else
            {
                User = task.Result.User;
                if(User != null)
                {
                    FirebaseUser user = auth.CurrentUser;
                    if(user != null)
                    {
                        UserProfile profile = new UserProfile { DisplayName = userName };
                        var profileTask = user.UpdateUserProfileAsync(profile);
                        yield return new WaitUntil(() => profileTask.IsCompleted);
                        if(profileTask.Exception != null)
                        {
                            Debug.LogWarning(message:$"Failed to register:{profileTask.Exception}");
                            FirebaseException profileEx= profileTask.Exception.GetBaseException() as FirebaseException;
                            AuthError profileErrorCode = (AuthError)profileEx.ErrorCode;
                            warningRegisterText.text = "Username Set Failed!";
                        }
                        else
                        {
                            UIManager.Instance.LoginPanel();
                            Debug.Log("User Profile Updated Successfully");
                            warningRegisterText.text = "";
                            StartCoroutine(SaveUserName());
                            StartCoroutine(SaveRewardData());
                            StartCoroutine(FirstInformation());
                        }
                    }
                }

            }

        }

    }

    private IEnumerator FirstInformation()
    {
        var DBTask = DBref.Child("users").Child(User.UserId).Child("AttackPower").SetValueAsync(0.1f);
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Save Task failed with {DBTask.Exception}");
        }
        else Debug.Log("Save Completed");

        var DBTask2 = DBref.Child("users").Child(User.UserId).Child("BestStage").SetValueAsync(1);
        yield return new WaitUntil(() => DBTask2.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Save Task failed with {DBTask2.Exception}");
        }
        else Debug.Log("Save Completed");
    }

    public void OnRegister()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, userNameRegisterField.text));
    }

    private void PlusFriend(string friendName)
    {
        // 현재 Friends 목록에서 중복 확인
        bool isFriendAlreadyExist = false;
        DBref.Child("users").Child(User.UserId).Child("Friends").GetValueAsync().ContinueWith(task =>
        {
            var snapshot = task.Result;
            foreach (var friendSnapshot in snapshot.Children)
            {
                string existingFriendName = friendSnapshot.GetValue(true).ToString();
                if (existingFriendName == friendName)
                {
                    // 이미 존재하는 친구이므로 중복 추가 방지
                    isFriendAlreadyExist = true;
                    break;
                }
            }

            if (!isFriendAlreadyExist)
            {
                string newFriendKey = DBref.Child("users").Child(User.UserId).Child("Friends").Push().Key;
                DBref.Child("users").Child(User.UserId).Child("Friends").Child(newFriendKey).SetValueAsync(friendName)
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                        {
                            Debug.Log($"{friendName} 친구 추가 됨");
                            friendsList.Add(friendName);
                            newfriends = true;
                        }
                    });
            }
            else
            {
                Debug.Log($"{friendName} 이미 친구 목록에 존재합니다.");
            }
        });
    }

    private IEnumerator Friends()
    {
        var DB = DBref.Child("users");
        int i = 0;
        var dbtask = DB.GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                Debug.LogError($"Error fetching user IDs: {task.Exception}");
            }
            else if (task.IsCompleted)
            {
                var snapshot = task.Result;

                // 모든 users의 하위 노드에 대해 반복
                foreach (var userSnapshot in snapshot.Children)
                {
                    // 각 user의 UserID 출력
                    if (userSnapshot.Child("UserName").Value.ToString() != userNameText.text)
                    {
                        string userID = userSnapshot.Child("UserName").Value.ToString();
                        userIds.Add(userID);

                    }
                }
            }
        });
        yield return new WaitUntil(() => dbtask.IsCompleted);
        foreach (Transform child in buttonParent)
        {
            // 자식 GameObject가 Button 컴포넌트를 가지고 있는지 확인
            Button buttonComponent = child.GetComponent<Button>();

            // Button 컴포넌트가 있다면 해당 자식 GameObject를 제거
            if (buttonComponent != null)
            {
                Destroy(child.gameObject);
            }
        }
        foreach (string userid in userIds)
        {
            Button friendButton = Instantiate(buttonprefab);
            friendButton.transform.SetParent(buttonParent);
            friendButton.transform.localPosition = new Vector3(0, 1 * -(i * 100), 0);
            friendButton.GetComponentInChildren<TMP_Text>().text = userid;
            friendButton.onClick.AddListener(() => PlusFriend(userid));
            i++;
        }
    }

    public void OnShowFriends()
    {
        UIManager.Instance.ShowFriends();
        userIds.Clear();
        if(UIManager.Instance.show)
            StartCoroutine(Friends());
    }

    private IEnumerator ChangePassword(string email, string password, string newPassword)
    {
        var user = auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
            {
                FirebaseUser user = task.Result.User;
                
                var passwordCheck = user.UpdatePasswordAsync(newPassword);
                if (passwordCheck.Exception != null)
                {
                    Debug.LogError($"Password update failed: {passwordCheck.Exception.Message}");
                    // 오류 처리
                }
            }
            else
            {
                Debug.LogError($"Sign-in failed: {task.Exception.Message}");
                // 로그인 실패 시 처리
            }
        });
        yield return new WaitUntil(() => user.IsCompleted);
        UIManager.Instance.LoginPanel();
        Debug.Log("User signed in successfully");
    }

    public void ChangePasswordButton()
    {
        StartCoroutine(ChangePassword(currentEmailRegisterField.text, currentPasswordRegisterField.text, changePasswordCheckRegisterField.text));
    }

    private IEnumerator TextFriends()
    {
        var task = DBref.Child("users").Child(User.UserId).Child("Friends").GetValueAsync()
                .ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        var snapshot = task.Result;

                        foreach (var userSnapshot in snapshot.Children)
                        {
                            Debug.Log(userSnapshot.GetValue(true).ToString());
                            friendsList.Add(userSnapshot.GetValue(true).ToString());
                        }
                    }
                });
        yield return new WaitUntil(() => task.IsCompleted);

        foreach (var names in friendsList)
            friends.text += names + '\n';
    }

    private IEnumerator GetAttackPower()
    {
        // 들고 오기 이상함 + 적 그리기 + 배경음
        var DBTask = DBref.Child("users").Child(User.UserId).Child("AttackPower").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
        GameManager.Instance.attackPower = float.Parse(DBTask.Result.Value.ToString());
    }

    private IEnumerator GetBestStage()
    {
        var DBTask = DBref.Child("users").Child(User.UserId).Child("BestStage").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
        GameManager.Instance.bestStage = int.Parse(DBTask.Result.Value.ToString());
    }

    public IEnumerator SaveAttackPower()
    {
        var DBTask = DBref.Child("users").Child(User.UserId).Child("AttackPower").SetValueAsync(GameManager.Instance.attackPower);
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Save Task failed with {DBTask.Exception}");
        }
        else Debug.Log("Save Completed");
    }

    public IEnumerator SaveBestStage()
    {
        var DBTask = DBref.Child("users").Child(User.UserId).Child("BestStage").SetValueAsync(GameManager.Instance.level);
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Save Task failed with {DBTask.Exception}");
        }
        else Debug.Log("Save Completed");
    }

    private IEnumerator Login(string email, string password)
    {
        var task = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => task.IsCompleted);
        
        if (task.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to Login:{task.Exception}");
            FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
            AuthError errorcode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorcode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not Exist";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            User = task.Result.User;
            Debug.Log($"User Signed in Successfully: {User.Email}, {User.DisplayName}");
            warningLoginText.text = "";

            //값 변경될 때 마다 이벤트 호출
            DBref.Child("users").Child(User.UserId).Child("LastLogin").ValueChanged += LoadLastLogin;

            StartCoroutine(SaveLoginData());
            StartCoroutine(GetAttackPower());
            StartCoroutine(GetBestStage());
            StartCoroutine(LoadUserName());
            StartCoroutine(GetLoginCount());
            StartCoroutine(TextFriends());
            UIManager.Instance.CloseLogin();
            StartCoroutine(LoadColor());
        }
    }

    private void LoadLastLogin(object sender, ValueChangedEventArgs e)
    {
        if(e.DatabaseError != null)
        {
            Debug.LogError(e.DatabaseError.Message);
            return;
        }
        else
        {
            DBref.Child("users").Child(User.UserId).Child("RewardLogin")
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        if(snapshot!=null && snapshot.Value != null)
                        {
                            strLastLogin = snapshot.Value.ToString();
                            Debug.Log($"Reward Login :{strLastLogin}");
                        }
                    }
                });
        }
    }

    public void OnRewardButton()
    {
        string date = GetNow();
        if (strLastLogin.Substring(0, 7).CompareTo(date.Substring(0, 7)) < 0)
        {
            strLastLogin = date;
            DBref.Child("users").Child(User.UserId).Child("RewardLogin").SetValueAsync(date)
                .ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        GameManager.Instance.attackPower += 0.01f;
                        Debug.Log($"Reward LoginDate Updated:{date}");
                        Debug.Log($"{loginCount}보상 받음");
                    }
                });
            
        }

    }

    public void LoginButton()
    {
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator SaveUserName()
    {
        var DBTask = DBref.Child("users").Child(User.UserId).Child("UserName")
            .SetValueAsync(userNameRegisterField.text);
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Save Task failed with {DBTask.Exception}");
        }
        else Debug.Log("Save Completed");
    }

    private IEnumerator LoadUserName()
    {
        var DBTask = DBref.Child("users").Child(User.UserId).Child("UserName")
            .GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Load Task failed with {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            Debug.Log("Load Completed");
            userNameText.text = $"{snapshot.Value}";
        }
    }

    //최초 회원가입 사용자 보상 초기화
    private IEnumerator SaveRewardData()
    {
        var DBTask = DBref.Child("users").Child(User.UserId).Child("RewardLogin")
            .SetValueAsync("00000000000000");
        DBTask = DBref.Child("users").Child(User.UserId).Child("CountRewardLogin")
            .SetValueAsync(0);
        DBref.Child("users").Child(User.UserId).Child("Friends")
            .SetValueAsync(null);
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to save task with {DBTask.Exception}");
        }
        else
        {
            Debug.Log("Reward Data Initailized");
        }
    }

    public string GetNow()
    {
        return DateTime.Now.ToString("yyyyMMddHHmmss");
    }

    private IEnumerator GetLoginCount()
    {
        var DBTaskGG = DBref.Child("users").Child(User.UserId).Child("CountRewardLogin").GetValueAsync();
        yield return new WaitUntil(() => DBTaskGG.IsCompleted);
        loginCount = int.Parse(DBTaskGG.Result.Value.ToString());

        loginCount++;
        var DBTaskS = DBref.Child("users").Child(User.UserId).Child("CountRewardLogin").SetValueAsync(loginCount);
        yield return new WaitUntil(() => DBTaskS.IsCompleted);

        var DBTaskG = DBref.Child("users").Child(User.UserId).Child("CountRewardLogin").GetValueAsync();
        yield return new WaitUntil(() => DBTaskG.IsCompleted);
        if (DBTaskG.Exception != null)
        {
            Debug.LogWarning($"Load Task failed with {DBTaskG.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTaskG.Result;
            Debug.Log("Load Completed");
            loginCountText.text = $"{loginCount}th Days Login!";
        }
    }

    private IEnumerator SaveLoginData()
    {
        string currentDateTime = GetNow();

        var DBTask = DBref.Child("users").Child(User.UserId).Child("LastLogin").SetValueAsync(currentDateTime);

        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to Save task with {DBTask.Exception}");

        }
        else
        {
            Debug.Log("Login Date update: " + currentDateTime);
        }
    }

    private IEnumerator ChangeColor()
    {
        int rand = UnityEngine.Random.Range(1, 3);
        switch (rand)
        {
            case 1:
                enemycolor = "red";
                break;
            case 2:
                enemycolor = "blue";
                break;
            default:
                enemycolor = "red";
                break;
        }
        var DBTaskSet = DBref.Child("Color").SetValueAsync(enemycolor);
        yield return new WaitUntil(() => DBTaskSet.IsCompleted);
        var DBTask = DBref.Child("Color").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to Load task with{DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            switch (snapshot.Value.ToString())
            {
                case "red":
                    enemyMaterial.color = Color.red;
                    break;
                case "blue":
                    enemyMaterial.color = Color.blue;
                    break;
            }
        }
        GameManager.Instance.powerUp = false;
        UIManager.Instance.changeColor = false;
    }
    private IEnumerator LoadColor()
    {
        int rand = UnityEngine.Random.Range(1, 3);
        switch (rand)
        {
            case 1:
                enemycolor = "red";
                break;
            case 2:
                enemycolor = "blue";
                break;
            default:
                enemycolor = "red";
                break;
        }
        var DBTaskSet = DBref.Child("Color").SetValueAsync(enemycolor);
        yield return new WaitUntil(() => DBTaskSet.IsCompleted);
        var DBTask = DBref.Child("Color").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
        if(DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to Load task with{DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;
            switch (snapshot.Value.ToString())
            {
                case "red":
                    enemyMaterial.color = Color.red;
                    break;
                case "blue":
                    enemyMaterial.color = Color.blue;
                    break;
            }
        }
    }

}
