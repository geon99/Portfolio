**언리얼 물리 반응 오브젝트 구현**

https://drive.google.com/file/d/1PETnk873ZLtHKYKVUSPQGYHcoLyO-daY/view?usp=sharing

- W,A,S,D : 캐릭터 이동
- 마우스 왼쪽버튼 : 수류탄 발사
- 마우스 오른쪽 버튼 : 발사체 (오브젝트 파괴)

<img src="https://raw.githubusercontent.com/geon99/Portfolio/master/Unreal%20Samples/Foliage/foliage.png" width=400/>
<img src="https://raw.githubusercontent.com/geon99/Portfolio/master/Unreal%20Samples/Foliage/fire.png" width=400/>
<img src="https://raw.githubusercontent.com/geon99/Portfolio/master/Unreal%20Samples/Foliage/snow.png" width=400/>
<img src="https://raw.githubusercontent.com/geon99/Portfolio/master/Unreal%20Samples/Foliage/destroy.png" width=400/>
<img src="https://raw.githubusercontent.com/geon99/Portfolio/master/Unreal%20Samples/Foliage/destructible.png" width=400/>

- 풀잎 충돌 효과; 캐릭터 또는 동적 오브젝트와 풀잎의충돌 효과 표현  (피직스 컨스트레인트 활용)
- 풀잎 발화 효과; 풀잎에 불을 피워 주변으로 번져 나아가는 것을 표현
- 눈위 이동 자국 내기: 캐릭터가 눈위를 이동시 눈이 눌려진 효과를 표현, 그리고 동적 오브잭트에 반응
- 오브잭트 파괴 효과: 오브잭트를 충돌 이벤트에 의해 파괴하고 풀잎 또는 눈 지형과 반응
- 오브젝트 파괴 효과: 발사체를 통한 오브젝트 파괴 효과



--------------------------------------------------------------

**유니티 라이트 테스트**

*라이트맵효과*

![](https://raw.github.com/geon99/Portfolio/master/Unity%20Light%20Test/sc1.png)

--------------------------------------------------------------

*실시간 라이트 효과 (Prob Light)*

![](https://raw.github.com/geon99/Portfolio/master/Unity%20Light%20Test/dsc1.png)
![](https://raw.github.com/geon99/Portfolio/master/Unity%20Light%20Test/dsc2.png)
[(동영상 링크)](https://raw.github.com/geon99/Portfolio/master/Unity%20Light%20Test/prob light.mp4)


--------------------------------------------------------------

**광대역 지형에서 자연물 오브젝트를 자동으로 생성하는 툴 (WorldTool_for_Unity)**

![설명](https://raw.github.com/geon99/Portfolio/master/WorldTool_for_Unity/worldt.png)
 - Unity 기반 툴
 - Worldmachine Tool 에서 생성한 지형 생성 정보를 기반으로 나무와 바위의 분포를 자연스럽게 생성한다.

--------------------------------------------------------------
**유니티용 게임 데이터 편집툴 (DataEdit_for_Unity)**
   
![설명](https://raw.github.com/geon99/Portfolio/master/DataEdit_for_Unity/tool%20sc.png)

- 각 아이템 별로 속성을 편집 
- 팝업 창으로 지정된 입력 값 선택 가능
- 아이템 벨런스 검사
- 속성별 전체 아이템 값의 비율 조정
- 데이터 값 암호화 저장
   
--------------------------------------------------------------
**비디오 메모리 사용량 분석 (VRAM_Report)**

![설명](https://raw.github.com/geon99/Portfolio/master/VRAM_Report/vram_reoprt.png)

비디오메모리 최적화를 위하여 각 렌더링 요소별로 사용되는 메모리 크기의 변화를 체크 하고 결과를 표와 그래프로 정리 하였다.

기존의 게임 엔진 소스에서 Direct3D 관련 리소스 생성의 프로파일링을 위해 모든 리소스 생성 함수를 캡슐화 하였다.

- 과도한 메모리 사용 요소 확인
- 비 정상적인 메모리 누적을 확인

--------------------------------------------------------------
**BugTrap 데이터 자동 수집 (BugTrap_Report)**

![설명](https://raw.github.com/geon99/Portfolio/master/BugTrap_Report/bug_reoprt.png)

- 개발배경

BugTrap을 통해서 생성된 오류 정보의 분석은 대량의 데이터를 불편한 수작업으로 할 경우 상당한 시간이 걸릴 수가 있다. 오류 발생 시 원인 분석을 보다 쉽게 하기 위하여 프로그램 상태 및 사용 환경 데이터를 
추가 수집하고, 수집 된 데이터를 일목요연하게 자동으로 정리해주는 툴을 개발 하였다.

- 적용효과

버그 리포트 생성을 자동화함으로써 작업 시간을 대폭 단축했습니다.
표로 생성된 리포트를 통해서 보다 더 쉽게 분석 하게 되었다.

--------------------------------------------------------------
**유니티를 사용하여 개발한 땅따먹기 게임 (Panic)**

![설명](https://raw.github.com/geon99/Portfolio/master/Panic/p-3.png)

 - 개인적으로 작업한 유니티 기반의 땅따먹기 게임 입니다.
 - 직접 설계 및 UI이미지 작업을 하였습니다. 
 - 작업 기간 약 2개월

--------------------------------------------------------------
**DirectX를 사용하여 C++에서 개발한 자체 제작한 게임 엔진 (MapEdit)**

<img src="https://raw.github.com/geon99/Portfolio/master/MapEdit/mape.png" width=700/>

[월드에디터]
 - File 텝
   - 지형과 오브젝트들의 세이브 로드 기능
   - 현재 편집 지역 설정
   - 태양광원 애니메이션 설정

 - Height 텝
   - 지형높이 처리관련기능
   - 브러쉬 모양 선택
   - 지면 올리기(내리기), 지면 평준화하기, 지형 평탄화 하기
   - 편집 강도 선택
   - 브러쉬 크기 조절
   - 지면 셰이더 선택(게임 셰이더, 디뷰즈 라이트, 노말맵, 높이맵)

 - 텍스쳐 텝
   - 지면의 텍스쳐를 선택하고, 알파이미지 그리기
   - 브러시 크기 조절
   - 브러시 강도 조절
   - 블렌딩택스쳐의 각 채널에 해당하는 택스쳐 선택

 - 오브젝트 텝
   - 월드에 오브젝트 배치를 편집한다.
   - 편집모드 선택 (오브젝트추가, 오브젝트 선택, 이동, 회전, 스케일)
   - 편집할 축 선택 (x,y,z)
   - 이동 시 자동으로 지면에 높이 고정
   - 이동 시 자동으로 오브젝트 위에 높이 고정
   - 오브젝트 모델 선택
