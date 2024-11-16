## 개인 학습

- [소켓통신 공부 📡](https://github.com/jiye21/Com2us_Chat/wiki/%EC%86%8C%EC%BC%93%ED%86%B5%EC%8B%A0-%EA%B3%B5%EB%B6%80)
---
<center><img  src="https://github.com/user-attachments/assets/c54d6fee-9280-4ebf-87ae-882ec5d0ae05"  alt="배너"  width="25%"/></center>


<br/>


# 1. Project Overview (프로젝트 개요)

- 프로젝트 이름: 나는 시민입니다.

- 프로젝트 설명: 로그인 기능이 있는 실시간 채팅서비스
> **시연영상:**

[![Video Label](https://img.youtube.com/vi/8JuIPQua21o/0.jpg)](https://youtu.be/8JuIPQua21o)
  

<br/>

<br/>

  

# 2. Team Members (팀원 및 팀 소개)


| 박지예 | 문지미 | 전준모 |
|:--:|:--:|:--:|
| <img  src="https://github.com/user-attachments/assets/4b410760-99a7-4a25-aabc-3c6bc3f654b4" width="100%"/><br /> <img  src="https://github.com/user-attachments/assets/8c4bc946-b4db-454a-82eb-7999bbd53d26" width="80%"/> | <img  src="https://github.com/user-attachments/assets/4b410760-99a7-4a25-aabc-3c6bc3f654b4" /> <br /> <img  src="https://github.com/user-attachments/assets/f8092671-8b82-4dc9-bba1-af4cefdd1a56" width="80%"/>| <img  src="https://github.com/user-attachments/assets/64edca2d-0c6f-46a7-be4f-b0c1578a387b" width="100%"/> <br /> <img  src="https://github.com/user-attachments/assets/666e53b7-ed30-4c35-b87c-0e4fb5f50342" width="90%"/>|
| FE, BE (실시간 서버) | BE (실시간 서버) | BE (인증 서버) |

<br/>

<br/>

  

# 3. Key Features (주요 기능)

>  **회원가입**:

- 회원가입 시 DB에 유저정보가 등록됩니다.


>  **로그인**:

- ID, PW를 입력받아 로그인합니다. 
  

>  **로비**:

- 로그인 시 바로 로비 화면이 나타납니다.
  
- 로비 화면에서 채팅방 목록을 보고 원하는 채팅방에 들어갈 수 있습니다. 
- 우측 상단의 버튼을 클릭해 새로운 채팅방을 만들 수도 있습니다. 

  

>  **채팅방**:

- 각 채팅방 당 최대 4명까지 입장 가능합니다. 

- 원하는 별명을 입력 후, 다른 유저와 자유롭게 대화를 나눌 수 있습니다. 
- 우측 상단의 x버튼으로 로비 화면으로 돌아갈 수 있습니다. 
- 채팅방에 남은 사람이 없을 경우 해당 채딩방은 목록에서 사라집니다. 

  

<br/>

<br/>

  

# 4. Tasks & Responsibilities (작업 및 역할 분담)

| **박지예** | <ul><li>팀장</li><li>프로젝트와 멘토링 일정 조율</li><li>실시간 서버 개발 <br />- 클라이언트 요청에 대한 응답 처리<br />- 채팅 서버와 연결된 클라이언트들 정보 처리<br />- 채팅방 목록 정보 처리</li><li>클라이언트 전반 개발</li></ul> |
|:--|:--|
| **문지미** | <ul><li>실시간 서버 개발<br />- 실시간 서버와 캐싱 서버의 연결 관리</li><li>로그인 화면 개발</li></ul> |
| **전준모** | <ul><li>인증 서버 개발<br />- 인증 서버와 AWS EC2 MySQL 연동<br />- 클라이언트에서 입력받은 로그인 정보로 알맞은 응답 생성 후 전달<br />- 세션키 발급 부분 개발</li></ul> |


<br/>

<br/>

# 5. Technology Stack (기술 스택)

## 5.1 Language

| C# | <img src="https://github.com/user-attachments/assets/cbf32ce1-adcf-4eed-b136-010129697892" width="100"> |
|-----------------|-----------------|


<br/>

## 5.2 Frontend

| Unity | <img src="https://github.com/user-attachments/assets/617a3fbd-4c97-43c7-b42f-5090358e9cb9" width="100"> | 2022.3.14f1 |
|-----------------|-----------------|-----------------|


<br/>

## 5.3 Backend

| ASP .NET | <img src="https://github.com/user-attachments/assets/753c0887-3114-4f24-85e8-27356dd86e72" width="130"> | 7.0 |
|-----------------|-----------------|-----------------|

<br/>


## 5.4 Cooperation

| | |
|-----------------|-----------------|
| Git | <img src="https://github.com/user-attachments/assets/483abc38-ed4d-487c-b43a-3963b33430e6" alt="git" width="100"> |
| Slack | <img src="https://github.com/user-attachments/assets/c415411f-0295-4943-9d37-53c08705705d" width="100"> |
| Notion | <img src="https://github.com/user-attachments/assets/34141eb9-deca-416a-a83f-ff9543cc2f9a" alt="Notion" width="100"> |


<br/>

# 6. Project Structure (프로젝트 구조)

```plaintext
├── ChatClient/
│  ├── Assets/  # 프로젝트에 사용된 에셋 모음
│  ├── ProjectSettings/  # 프로젝트의 전반적인 설정 파일 폴더
│  ├── build/  # 빌드 파일이 들어있는 폴더
│  │   ├── ...
│  │   ├── Chatting_Client.exe  # 채팅 서비스(클라이언트) 실행 파일
├── Chat_Server_Room/
│  ├── Properties/
│  ├── App.config
│  ├── Chat_Server_Room.csproj
│  ├── Program.cs  # 실시간 서버의 메인 코드 파일
│  ├── packages.config  # nuget 패키지들
├── Web_API_Server/
│  ├── Config/Database/Init/account.sql  # DB Init 파일
│  ├── Controllers/
│  │   ├── DTO/Account.cs
│  │   ├── AccountController.cs
│  ├── Database/
│  │   ├── Database.cs
│  ├── Properties/
│  │   ├── launchSettings.json
│  ├── obj/Release/net7.0/
│  │   ├── ...
│  ├── ErrorCode.cs  # 에러 발생 시 보낼 응답 코드
│  ├── Program.cs  # 애플리케이션의 시작점. 호스팅 설정, 서비스 구성 등을 함
│  ├── ...
│  ├── appsettings.json  # DB 접속 주소 설정 파일
├── .gitignore
└── README.md

```

  

<br/>

<br/>
