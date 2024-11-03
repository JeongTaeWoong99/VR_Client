using System.Collections;
using System.Threading.Tasks;
using Firebase.Storage;
using Photon.Pun;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.SceneManagement;

// 로비 // 비디오 체크 및 다운로드
public class VideoManager : MonoBehaviour
{
    public static VideoManager instance;
    
    private FirebaseStorage  storage;
    private StorageReference stRef;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // storage 세팅
        storage = FirebaseStorage.DefaultInstance;
        stRef   = storage.GetReferenceFromUrl("gs://cms-login-d93aa.appspot.com/");
        
        // FileBrowser 세팅
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "Images", ".jpg", ".png" ), new FileBrowser.Filter( "Text Files", ".txt", ".pdf" ),
                                                             new FileBrowser.Filter( "Video Files", ".mp4"));
        FileBrowser.SetDefaultFilter(".mp4");                                        // 기본 필터를 mp4로 설정
        FileBrowser.SetExcludedExtensions( ".lnk", ".tmp", ".zip", ".rar", ".exe" ); // 검색 제외
        FileBrowser.AddQuickLink( "Users", "C:\\Users", null);          // 기존 위치
    }

    // 입장 버튼(비디오 체크 및 입장)
    public IEnumerator CheckAndJoin(string videoName,string roomName)
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, null, null, "Select Files", "Load");
        Debug.Log(FileBrowser.Success);
        
        if (FileBrowser.Result != null)
        {
            string[] filePaths = FileBrowser.Result;
            if (filePaths.Length > 0)
            {
                string filePath = filePaths[0];
                string fileName = FileBrowserHelpers.GetFilename(filePath); // 파일이름
                
                if (fileName == videoName + ".mp4")
                {
                    Debug.Log("선택한 동영상이 동일합니다.");
                    
                    PlayerPrefs.SetString("roomName",roomName);  // 공유름 씬을 로드하고, 포톤룸을 들어감.
                    PlayerPrefs.SetString("videoPath",filePath); // 파일 경로 정적 저장.
                    
                    SceneManager.LoadScene("360VideoScene");     // 씬 로드
                }
                else
                {
                    Debug.Log("선택한 동영상이 동일하지 않습니다.");
                    // 방 입장 옆에, 다운로드 버튼 하나 더 만들기...
                }
            }
        }
    }

    // 비디오 다운로드 버튼
    public IEnumerator VideoDownload(string videoName)
    {
        string desktopPath   = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);  // 데스크탑 바탕화면
        string localFilePath = System.IO.Path.Combine(desktopPath, videoName + ".mp4");                     // 저장 및 파일이름 세팅

        StorageReference videoRef = stRef.Child(videoName + ".mp4");    // 저장소 참조 위치

        Task downloadTask = videoRef.GetFileAsync(localFilePath);        // 비동기 Task 실행
    
        yield return new WaitUntil(() => downloadTask.IsCompleted);

        if (downloadTask.IsFaulted || downloadTask.IsCanceled)
        {
            Debug.LogError("다운 실패 : " + downloadTask.Exception);
        }
        else
        {
            Debug.Log("다운 성공 : " + localFilePath);
        }
    }
}
