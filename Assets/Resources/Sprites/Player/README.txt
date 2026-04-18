[ Player 스프라이트 폴더 ]

이 폴더에 PNG 파일을 넣으면 자동으로 플레이어에 적용됩니다.

─────────────────────────────────────────
파일명 규칙
─────────────────────────────────────────

■ 단일 프레임
  idle.png
  walk.png
  jump.png

■ 다중 프레임 (애니메이션)
  idle_0.png  idle_1.png  idle_2.png ...
  walk_0.png  walk_1.png  walk_2.png ...
  jump_0.png  jump_1.png  ...

─────────────────────────────────────────
Unity Import 설정 (Sprite 추가 후 Inspector)
─────────────────────────────────────────
  Texture Type : Sprite (2D and UI)
  Pixels Per Unit : 32   ← 프로젝트 기준값
  Filter Mode : Point (픽셀아트 권장)
  Compression : None

─────────────────────────────────────────
애니메이션 속도 조절
─────────────────────────────────────────
  CharacterAnimator 컴포넌트의 Anim Fps 값을 변경하세요. (기본 8fps)
