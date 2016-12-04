import socket, sys

HOST = '127.0.0.1'
PORT = 12345
try:
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
except socket.error as msg:
    sys.stderr.write("[ERROR] %s\n" % msg[1])
    sys.exit(1)

sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1) #reuse tcp
sock.bind((HOST, PORT))
sock.listen(5)
#sock.settimeout(10)

(csock, adr) = sock.accept()
print("Client Info: ", csock, adr)

while True:
    print("I'm working...")
    try:
        Confirm = csock.recv(1024)
    except socket.error as msg:
        sys.stderr.write("[ERROR] %s\n" % msg[1])
        while True:
            pass
    if(Confirm == 'END'):
        break
    elif(Confirm == '0' or Confirm == '1'):
        pass
    else:
        print('Confirm Error!\n')
        while True:
            pass
    
    csock.send('123 -2321 -3233\0')

csock.close()