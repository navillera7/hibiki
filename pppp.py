import bcrypt

# 1. 사용할 새 비밀번호를 입력하세요
new_password = "gaechoo12"

# 2. 비밀번호를 해싱합니다 (C#의 BCrypt와 동일한 알고리즘)
# .encode('utf-8')은 문자열을 바이트 형태로 변환합니다.
hashed = bcrypt.hashpw(new_password.encode('utf-8'), bcrypt.gensalt())

# 3. 생성된 해시값을 출력합니다
print("\n생성된 해시값 (이 문자열을 복사하세요):")
print(hashed.decode('utf-8'))