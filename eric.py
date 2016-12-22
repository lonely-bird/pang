import socket
import sys
import time

### Let the variable 0 to avoid SelfTesting
__Interface__SelfTest = 0
###
__Interface__game_side_time = 0.0
__Interface__AI_side_time = 0.0
__Interface__global_time_clock = time.clock()
__Interface__eps = 0.5
__Interface__TransitDataSize = 1024
__Interface__MessagePartitionSymbol = '\r\n'
__Interface__FeedbackPartitionSymbol = ' '
__Interface__HOST = '127.0.0.1'
__Interface__PORT = 5

def __Interface__SocketInit():
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    except socket.error as msg:
        sys.stderr.write("[ERROR] %s\n" % msg[1])
        while True:
            pass
    
    try:
        sock.connect((__Interface__HOST, __Interface__PORT))
    except socket.error as msg:
        sys.stderr.write("[ERROR] %s\n" % msg[1])
        while True:
            pass
    
    return sock

def __Interface__ParseFeedback(Buffer):
    FeedBack = []
    while True:
        if len(Buffer) == 0:
            break
        NowProcess, Ignore, Buffer = Buffer.partition(__Interface__FeedbackPartitionSymbol)
        FeedBack.append(float(NowProcess))
    done = FeedBack[0]< __Interface__eps
    reward = FeedBack[0]
    _ = done
    observation = []
    toappend = [1,2,4,5,6,7]
    for i in toappend:
        observation.append(FeedBack[i])

    Return_List = [observation,reward,done,_]
    return Return_List

def step(Motion):
    t0 = time.clock()
    global __Interface__global_time_clock
    global __Interface__AI_side_time
    __Interface__AI_side_time += t0 - __Interface__global_time_clock
    Buffer = ''
    try:
        __Interface__Sock.send((str(Motion) + __Interface__MessagePartitionSymbol).encode())
    except socket.error as msg:
        sys.stderr.write("[ERROR] %s\n" % msg[1])
        while True:
            pass

    while True:
        try:
            NewComingdata = __Interface__Sock.recv(__Interface__TransitDataSize)
        except socket.error as msg:
           sys.stderr.write("[ERROR] %s\n" % msg[1])
           while True:
               pass
        
        if not NewComingdata:
            print('Message Transition Error!: No coming data')
            while True:
                pass
        
        Buffer += NewComingdata.decode()
        if __Interface__MessagePartitionSymbol not in Buffer:
            continue
        
        if(Buffer[len(Buffer)-len(__Interface__MessagePartitionSymbol):len(Buffer)] != __Interface__MessagePartitionSymbol):
            print('Message Partition Error!: Last char should be %s' % (__Interface__MessagePartitionSymbol))
            while True:
                pass
        
        Buffer, Ignore , ShouldBeEmpty = Buffer.partition(__Interface__MessagePartitionSymbol)

        ret = __Interface__ParseFeedback(Buffer)        
        
        t1 = time.clock()
        __Interface__global_time_clock = t1
        global __Interface__game_side_time
        __Interface__game_side_time += t1-t0

        return ret

__Interface__Sock = __Interface__SocketInit()
def Disconnect():
    global __Interface__Sock
    __Interface__Sock.close()
def Connect():
    return 
    global __Interface__Sock
    __Interface__Sock = __Interface__SocketInit()
def render(): pass

def reset():
    return step('R')[0]


def __Interface__FeedBackPrint(FeedBack):
    print('FeedBacks:'),
    for i in FeedBack:
        if (type(i)!=list):
            print ('%f' %(i))
        else:
            for j in i:
                print ('%f' % (j)),
    print 

if __Interface__SelfTest:
    FeedBack = step('R')
    __Interface__FeedBackPrint(FeedBack)
    FeedBack = step(0)
    __Interface__FeedBackPrint(FeedBack)
    print
