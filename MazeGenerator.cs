using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour {

	public GameObject blockPrefab;

	public int mazeWidth;
	public int mazeHeight;
	public Color mazeColor0;
	public Color mazeColor1;

	private GameObject[,] blocks;
	private IEnumerator currentCoroutine;
	private float fixedGapTime = 0.05f;

	public void StartGenerateMaze () {
		if (mazeWidth < 2 || mazeHeight < 2)	// 최소 5 * 5
			return;
		
		if (currentCoroutine != null)			// 중복 코루틴 요청 무시하지 않기 위해
			StopCoroutine (currentCoroutine);
		
		currentCoroutine = GenerateMaze (mazeWidth, mazeHeight, mazeColor0, mazeColor1);
		StartCoroutine (currentCoroutine);
	}

	IEnumerator GenerateMaze (int width, int height, Color color0, Color color1) {
		// 그 전에 만든 미로가 있으면 지움
		if (blocks != null) {
			for (int y = 0; y < blocks.GetLength (1); y++) {
				for (int x = 0; x < blocks.GetLength (0); x++) {
					Destroy (blocks [y, x].gameObject);
				}
			}
		}

		// 빈 공간, 벽 모두 블록 따위로 둘 것임
		int totalWidth = (width * 2) + 1;
		int totalHeight = (height * 2) + 1;

		// 단순한 카메라 워킹
		GameObject.FindWithTag ("MainCamera").GetComponent<CameraMove> ().SetDesiredPosition (totalWidth, totalHeight);

		int [,] intBlocks  = new int [totalHeight, totalWidth];	// 0: 빈 공간, 1: 벽, 2: 검사한 공간
		blocks = new GameObject[totalHeight, totalWidth];

		for (int y = 0; y < totalHeight; y++) {
			for (int x = 0; x < totalWidth; x++) {
				GameObject block = Instantiate (blockPrefab);

				// 입구와 출구를 제외하고 겉부분이거나 x, y좌표 중 하나가 짝수일 때 벽 생성
				// 그 이외는 빈 공간 생성
				if (x == 0 || x == totalWidth - 1 || y == 0 || y == totalHeight - 1 || x % 2 == 0 || y % 2 == 0) {
					intBlocks [y, x] = 1;
					block.GetComponent<MeshRenderer> ().material.color = color1;
				} else {
					intBlocks [y, x] = 0;
					block.GetComponent<MeshRenderer> ().material.color = color0;
				}

				block.transform.position = new Vector2 (x, y);
				block.name = "Block["+x+", "+y+"]";
				block.transform.SetParent (gameObject.transform);
				blocks [y, x] = block;
			}
		}

		// 입구와
		intBlocks [0, 1] = 0;
		blocks[0, 1].GetComponent<MeshRenderer> ().material.color = color0;
		// 출구
		intBlocks [totalHeight-1, totalWidth-2] = 0;
		blocks[totalHeight-1, totalWidth-2].GetComponent<MeshRenderer> ().material.color = color0;

		// 첫 시작
		// x, y좌표 모두 짝수 인 경우 무조건 벽
		int currentX, currentY;
		do {
			currentX = Random.Range (1, totalWidth - 1);
		} while (currentX % 2 == 0);
		do {
			currentY = Random.Range (1, totalHeight - 1);
		} while (currentY % 2 == 0);

		while (true) {
			// 정해진 블록으로부터 가능한 멀리 길을 만듦
			// 블록 2 단위 조건: 이미 검색한 공간인지
			// 블록 1 단위 연산: 벽 뚫기
			do {
				intBlocks [currentY, currentX] = 2;

				bool top = true;		// 0
				bool right = true;		// 1
				bool bottom = true;		// 2
				bool left = true;		// 3

				int currentDirection = Random.Range(0, 4);
				int currentDirectionDelta = 0;
				do {
					currentDirectionDelta = Random.Range(1, 4);
				} while (currentDirectionDelta == 2);

				while (top || right || bottom || left) {
					bool determindDirection = false;
					switch (currentDirection) {
					case 0 :
						if (currentY + 2 < totalHeight - 1 && intBlocks[currentY+2, currentX] == 0) {
							intBlocks [currentY+1, currentX] = 0;
							blocks[currentY+1, currentX].GetComponent<MeshRenderer>().material.color = color0;

							currentY += 2;

							determindDirection = true;
						} else {
							top = false;
						}
						break;
					case 1 :
						if (currentX + 2 < totalWidth - 1 && intBlocks[currentY, currentX+2] == 0) {
							intBlocks [currentY, currentX+1] = 0;
							blocks[currentY, currentX+1].GetComponent<MeshRenderer>().material.color = color0;

							currentX += 2;

							determindDirection = true;
						} else {
							right = false;
						}
						break;
					case 2 :
						if (currentY - 2 > 0 && intBlocks[currentY-2, currentX] == 0) {
							intBlocks [currentY-1, currentX] = 0;
							blocks[currentY-1, currentX].GetComponent<MeshRenderer>().material.color = color0;

							currentY -= 2;

							determindDirection = true;
						} else {
							bottom = false;
						}
						break;
					case 3 :
						if (currentX - 2 > 0 && intBlocks[currentY, currentX-2] == 0) {
							intBlocks [currentY, currentX-1] = 0;
							blocks[currentY, currentX-1].GetComponent<MeshRenderer>().material.color = color0;

							currentX -= 2;

							determindDirection = true;
						} else {
							left = false;
						}
						break;
					}
					if (determindDirection) {
						yield return new WaitForSeconds (fixedGapTime);
						break;
					} else {
						currentDirection += currentDirectionDelta;
						currentDirection %= 4;
					}
				}

				if (!top && !right && !bottom && !left)
					break;
				
			} while (true);

			bool detect = false;

			// 막혀서 더 이상 길을 만들 수 없다면
			// 새롭게 시작할 블록을 찾음
			for (int y = 1; y < totalHeight && !detect; y += 2) {
				for (int x = 1; x < totalWidth  && !detect; x += 2) {
					if (intBlocks [y, x] == 0) {
						if (x + 2 < totalWidth && intBlocks [y, x + 2] == 2) {
							intBlocks [y, x] = 2;
							intBlocks [y, x + 1] = 0;
							blocks [y, x + 1].GetComponent<MeshRenderer> ().material.color = color0;

							currentX = x + 2;
							currentY = y;

							detect = true;
						} else if (y + 2 < totalHeight && intBlocks [y + 2, x] == 2) {
							intBlocks [y, x] = 2;
							intBlocks [y + 1, x] = 0;
							blocks [y + 1, x].GetComponent<MeshRenderer> ().material.color = color0;

							currentX = x;
							currentY = y + 2;

							detect = true;
						} else if (x - 2 > 0 && intBlocks [y, x - 2] == 2) {
							intBlocks [y, x] = 2;
							intBlocks [y, x - 1] = 0;
							blocks [y, x - 1].GetComponent<MeshRenderer> ().material.color = color0;

							currentX = x - 2;
							currentY = y;

							detect = true;
						} else if (y - 2 > 0 && intBlocks [y - 2, x] == 2) {
							intBlocks [y, x] = 2;
							intBlocks [y - 1, x] = 0;
							blocks [y - 1, x].GetComponent<MeshRenderer> ().material.color = color0;

							currentX = x;
							currentY = y - 2;

							detect = true;
						}
					}
				}
			}

			if (!detect) {
				break;
			} else {
				yield return new WaitForSeconds (fixedGapTime);
			}
		}
	}
}
