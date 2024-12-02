using System;
using System.Collections;
using System.Threading.Tasks;
using Firebase.Storage;
using SimpleFileBrowser;
using UnityEngine;
using Firebase.Extensions;
using TMPro;
using UnityEngine.SceneManagement;

// 로비 // 비디오 체크 및 다운로드
public class VideoManager : MonoBehaviour
{
    // private FirebaseStorage  storage;
    // private StorageReference stRef;
    //
    // public GameObject      settingScreen;
    // public TextMeshProUGUI settingText;
    //
    // [HideInInspector] 
    // public bool isVideoSetting;
    //
    //
    // private void Start()
    // {
    //     // storage 세팅
    //     storage = FirebaseStorage.DefaultInstance;
    //     stRef   = storage.GetReferenceFromUrl("gs://cms-login-d93aa.appspot.com/");
    //     
    //     // FileBrowser 세팅
    //     FileBrowser.SetFilters( true, new FileBrowser.Filter( "Images", ".jpg", ".png" ), new FileBrowser.Filter( "Text Files", ".txt", ".pdf" ),
    //                                                          new FileBrowser.Filter( "Video Files", ".mp4"));
    //     FileBrowser.SetDefaultFilter(".mp4");                                        // 기본 필터를 mp4로 설정
    //     FileBrowser.SetExcludedExtensions( ".lnk", ".tmp", ".zip", ".rar", ".exe" ); // 검색 제외
    //     FileBrowser.AddQuickLink( "Users", "C:\\Users", null);          // 기존 위치
    // }
    //
    //
    // // 입장 버튼(비디오 체크 및 입장)
    // public IEnumerator CheckAndJoin(string videoName,string roomName)
    // {
    //     yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, null, null, "Select Files", "Load");
    //     Debug.Log(FileBrowser.Success);
    //
    //     if (FileBrowser.Result != null && FileBrowser.Result.Length > 0)
    //     {
    //         string filePath      = FileBrowser.Result[0];                          // 파일경로
    //         string fileName      = FileBrowserHelpers.GetFilename(filePath);       // 파일이름
    //         
    //         CheckVideo(filePath,fileName,videoName,roomName);
    //     }
    // }
    //
    // private void CheckVideo(string filePath, string fileName, string videoName, string roomName)
    // {
    //     PunSystem.instance.loadingScreen.SetActive(true);
    //     PunSystem.instance.feedbackText.gameObject.SetActive(true);
    //     PunSystem.instance.feedbackText.text = "선택된 동영상을 체크하고 있습니다.";
    //
    //     // 간편 체크(파일이름 동일)
    //     if (fileName == videoName + ".mp4")
    //     {
    //         byte[] bytes         = FileBrowserHelpers.ReadBytesFromFile(filePath); // 파일정보
    //         string localFileHash = CalculateMD5Hash(bytes);                        // 파일의 MD5 해쉬 정보 계산(단방향 / 덜 안전 / 빠름 / 체크성)       
    //
    //         Debug.Log($"Local file hash: {localFileHash}"); // Output the local file hash
    //
    //         StorageReference storageRef = stRef.Child(fileName);
    //         storageRef.GetMetadataAsync().ContinueWithOnMainThread((metadataTask) =>
    //         {
    //             // 파일 존재 X(같은 이름의 파일이 저장소에 존재하지 않음.)
    //             if (metadataTask.IsFaulted || metadataTask.IsCanceled)
    //             {
    //                 // = 씬로드 X
    //                 UnityMainThreadDispatcher.instance.result = "저장소에 파일이 존재하지 않습니다. CMS 관리자의 파일 업로드가 필요합니다.";
    //                 UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
    //                 UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
    //             }
    //             // 파일 존재 O(같은 이름의 파일이 저장소에 존재함.)
    //             else
    //             {
    //                 StorageMetadata metadata       = metadataTask.Result;                      // Get metadata
    //                 string          remoteFileHash = metadata.GetCustomMetadata("md5Hash"); // Access the stored hash
    //                 
    //                 Debug.Log($"Remote file hash: {remoteFileHash}"); // Output the remote file hash
    //
    //                 // 파일 이름 동일 + 해쉬 동일 = 씬로드 O
    //                 if (remoteFileHash != null && remoteFileHash.Equals(localFileHash, StringComparison.OrdinalIgnoreCase))
    //                 {
    //                     UnityMainThreadDispatcher.instance.result = "저장소에 파일이 존재하며, 해쉬도 동일합니다. 씬을 로드합니다.";
    //                     UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
    //
    //                     PlayerPrefs.SetString("roomName", roomName);
    //                     PlayerPrefs.SetString("videoPath", filePath);
    //                     SceneManager.LoadScene("Screen Sharing");
    //                 }
    //                 // 파일 이름 동일 + 해쉬 다름 = 씬로드 X
    //                 else
    //                 {
    //                     UnityMainThreadDispatcher.instance.result = "저장소에 파일이 존재 하지만, 해쉬가 다릅니다.";
    //                     UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
    //                     UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
    //                 }
    //             }
    //         });
    //     }
    //     // 간편 체크(파일이름 다름)
    //     else
    //     {
    //         UnityMainThreadDispatcher.instance.result = "파일 이름이 다릅니다. 동일한 동영상을 선택해 주시길 바랍니다.";
    //         UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
    //         UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
    //     }
    // }
    //     
    // // MD5해쉬 계산
    // private string CalculateMD5Hash(byte[] bytes)
    // {
    //     using var md5 = System.Security.Cryptography.MD5.Create();
    //     byte[] hashBytes = md5.ComputeHash(bytes);
    //     return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    // }
    //
    // // 일반 큐 메서드
    // private void QueueFeedbackText() // 큐(메인 스레드)에서 작동.
    // {
    //     settingText.gameObject.SetActive(true);
    //     settingText.text = UnityMainThreadDispatcher.instance.result;
    // }
    //
    // // 비디오 다운로드 버튼
    // public IEnumerator VideoDownload(string videoName)
    // {
    //     PunSystem.instance.loadingScreen.SetActive(true);
    //     PunSystem.instance.feedbackText.gameObject.SetActive(true);
    //     PunSystem.instance.feedbackText.text = "동영상을 다운받고 있습니다.";
    //
    //     // 각각의 바탕화면에 저장하도록 함.
    //     string localFilePath;
    //     
    //     // 에디터 or 윈도우 빌드
    //     if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
    //     {
    //         string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    //         localFilePath      = System.IO.Path.Combine(desktopPath, videoName + ".mp4"); // For desktop builds
    //     }
    //     // VR빌드(모바일)
    //     else
    //     {
    //         string downloadPath = "/storage/emulated/0/Download";
    //         localFilePath       = System.IO.Path.Combine(downloadPath, videoName + ".mp4");
    //     }
    //
    //     StorageReference videoRef = stRef.Child(videoName + ".mp4");    // 저장소 참조 위치
    //     
    //     Task downloadTask = videoRef.GetFileAsync(localFilePath);       // 비동기 Task 실행
    //
    //     yield return new WaitUntil(() => downloadTask.IsCompleted);
    //
    //     if (downloadTask.IsFaulted || downloadTask.IsCanceled)
    //     {
    //         UnityMainThreadDispatcher.instance.result = downloadTask.Exception + " : 에러 발생";
    //         UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
    //         UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
    //     }
    //     else
    //     {
    //         UnityMainThreadDispatcher.instance.result = "다운 성공 : " + localFilePath;
    //         UnityMainThreadDispatcher.instance.MethodEnqueue(QueueFeedbackText);
    //         UnityMainThreadDispatcher.instance.MethodEnqueue(() => PunSystem.instance.loadingScreen.SetActive(false));
    //     }
    // }
}
