import bcrypt

new_password = "baechoo12".encode('utf-8')
# Work Factor를 12로 설정하여 해시 생성
hashed = bcrypt.hashpw(new_password, bcrypt.gensalt(rounds=12))

print(hashed.decode('utf-8'))