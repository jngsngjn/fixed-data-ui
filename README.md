# fixed-data-ui

fixed 길이 데이터를 생성하고 결과를 복사하는 Windows UI입니다.

## 빌드

```bat
build.bat
```

빌드 결과:

```text
dist\FixedDataUi.exe
```

앱 아이콘은 `assets\app.ico`가 실행 파일에 포함됩니다.

## 실행

```bat
dist\FixedDataUi.exe
```

## 사용 방법

- `length`에는 1 이상의 정수를 입력합니다.
- `value`에는 해당 필드 값을 입력합니다. 빈 값도 사용할 수 있습니다.
- `여백 방향`에서 값을 왼쪽에 붙일지 오른쪽에 붙일지 선택합니다.
- `생성`을 누르면 각 값을 선택한 방향으로 붙이고 남은 길이를 공백으로 채워 하나의 문자열로 이어 붙입니다.
- 결과 영역 오른쪽 위에서 UTF-8 기준 바이트 수를 확인할 수 있습니다.
- `복사`를 누르면 생성 결과가 클립보드에 복사됩니다.
- 프로그램 우하단에는 `made by jngsngjn`이 표시됩니다.

예시:

| length | value |
| ---: | --- |
| 10 | ABC |
| 5 | ABC |

결과:

```text
ABC       ABC  
```

## 실행 조건

Java는 필요하지 않습니다.

Windows의 .NET Framework 4.x가 필요합니다.
