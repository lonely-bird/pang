CFLAGS  += -std=c++14 -Wall -Wextra -Wshadow -g -O3 -fopenmp
LDFLAGS += -fopenmp
LIBS    += -lncurses
TARGET  = lolipop
OBJECTS = lolipop.o emulator.o learning.o

.PHONY : all clean
all : $(TARGET)

$(TARGET) : $(OBJECTS)
	$(CXX) $(LDFLAGS) $(OBJECTS) -o $@ $(LIBS)

lolipop.o: emulator.h learning.h
emulator.o: emulator.h
learning.o: learning.h

$(OBJECTS): %.o : %.cpp
	$(CXX) $(CFLAGS) -c $*.cpp -o $*.o

clean :
	$(RM) $(TARGET) $(OBJECTS)
