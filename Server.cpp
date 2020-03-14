#include <cstdio>
#include <cstring>
#include <cstdlib>
#include <unistd.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <sys/types.h>
#include <sys/epoll.h>
#include <errno.h>
#include <string>
#include <list>
#include <iostream>
#include <map>
#include <fcntl.h>
#include <sys/time.h>
#include <set>

#include "protobuf_low.pb.h"

#define MAXEVENTS 64

struct Server{
    int port;
    int listen_fd;
    int epoll_fd;
    int headLength;
    int nowHeadLength;
    int bodyLength;
    int nowBodyLength;
    struct epoll_event event;
    struct epoll_event* events;
    FILE *fp1, *fp2, *fp3, *fp4, *fp5;
    std::list<int> clientlist;
    std::set<int> loadlist;
    std::map<int,std::string> socketlist;
    std::map<int,Msg> movelist;
    std::string str;
    std::vector<std::vector<std::string> > vvs;
    Msg msg;
    struct timeval startTime, nowTime;
    bool gameStart;
    int framenum;
    void Init(){
        this->port = 3360;
        (this->clientlist).clear();
        this->headLength = 4;
        this->nowHeadLength = 0;
        this->bodyLength = 0;
        this->nowBodyLength = 0;
        this->events = (struct epoll_event*) malloc (sizeof(event)*MAXEVENTS);
        this->gameStart = false;
        this->framenum = 0;
        (this->vvs).clear();
        (this->loadlist).clear();
    }
    void Release(){
        if((this->events) != NULL) free(this->events);
        int ret = close(listen_fd);
        if(ret == -1){
            printf("listen_fd %d close error, errno = %d (%s)\n", listen_fd, errno, strerror(errno));
        }
        ret = close(epoll_fd);
        if(ret == -1){
            printf("epoll_fd %d close error, errno = %d (%s)\n", listen_fd, errno, strerror(errno));
        }
    }
    void SetPort(int _port){
        this->port = _port;
    }
    void Send(int fd, char *data, int len, int type){
        int ret = send(fd, data, len , 0);
        if(ret == -1){
            printf("send to socket: %d error, errno = %d (%s)\n",fd, errno, strerror(errno));
        }
        printf("send type = %d\n", type);
        for(int i = 0; i < len; i ++ ) printf("%u ", data[i]); printf("\n");
    }
    void Send(int fd, unsigned char *data, int len, int type){
        int ret = send(fd, data, len , 0);
        if(ret == -1){
            printf("send to socket: %d error, errno = %d (%s)\n",fd, errno, strerror(errno));
        }
        printf("send type = %d\n", type);
        for(int i = 0; i < len; i ++ ) printf("%u ", data[i]); printf("\n");
    }
    bool SetServerSocket(){
        listen_fd = socket(AF_INET, SOCK_STREAM, 0);
        printf("listen: %d\n", listen_fd);
        if(listen_fd == -1){
            printf("create server socket error, errno = %d, (%s)\n", errno, strerror(errno));
            return false;
        }

        // address
        struct sockaddr_in listen_addr;
        listen_addr.sin_family = AF_INET;
        listen_addr.sin_addr.s_addr = htonl(INADDR_ANY);
        listen_addr.sin_port = htons(port);

        // bind
        int bind_ret = bind(listen_fd, (struct sockaddr*)&listen_addr, sizeof(listen_addr));
        if(bind_ret == -1){
            printf("bind error, errno = %d, (%s)\n", errno, strerror(errno));
            return false;
        } else {
            printf("bind ret: %d\n", bind_ret);
        }

        // listen
        int listen_ret = listen(listen_fd, 128);
        if(listen_ret == -1){
            printf("listen error, errno = %d (%s)\n", errno, strerror(errno));
            return false;
        } else {
            printf("listen ret: %d\n", listen_ret);
        }

        // epoll
        epoll_fd = epoll_create(10);
        if(epoll_fd == -1){
            printf("epoll create error, errno = %d (%s)\n", errno, strerror(errno));
            return false;
        }
        event.data.fd = listen_fd;
        event.events = EPOLLIN;
        
        int ret = epoll_ctl(epoll_fd, EPOLL_CTL_ADD, listen_fd, &event);
        if(ret == -1){
            printf("epoll ctl error, errno = %d (%s)\n", errno, strerror(errno));
            return false;
        }
        return true;
    }
    bool GetDataStream(int fd, char *data){
        bodyLength = BytesToInt(data);
        //printf("%d\n", bodyLength);
        nowBodyLength = 0;
        char *buff = (char*) malloc (sizeof(char)*bodyLength);
        while(nowBodyLength < bodyLength){
            int ret = recv(fd, buff + nowBodyLength, bodyLength - nowBodyLength, 0);
            if(ret == -1){
                printf("recv error, errno = %d (%s)\n", errno, strerror(errno));
                return false;
            }
            nowBodyLength += ret;
        }
        str = "";
        for(int i = 0; i < bodyLength; i++ ) str += buff[i];
        free(buff);
        msg.ParseFromString(str);
        //std::cout << msg.optype() << '\n' << msg.username() << '\n' << msg.userpwd() << '\n';
        return true;
    }
    void InitPath(char **logpath, char **pospath, char **tagpath, char **rotpath){
        std::string a = "/root/user_list/";
        std::string b = "/root/user_pos/";
        std::string c = "/root/user_login/";
        std::string d = "/root/user_rot/";
        std::string e = msg.username();
        std::string f = ".txt";

        std::string logpath_str = a + e + f;
        std::string pospath_str = b + e + f;
        std::string tagpath_str = c + e + f;
        std::string rotpath_str = d + e + f;

        *logpath = (char*)logpath_str.c_str();
        *pospath = (char*)pospath_str.c_str();
        *tagpath = (char*)tagpath_str.c_str();
        *rotpath = (char*)rotpath_str.c_str();
    }
    std::string IntToString(int num){
        std::string ret = "";
        if(num < 0){
            ret += '-';
            num = -num;
        }
        if(num > 9) ret += IntToString(num / 10);
        ret += '0' + num % 10;
        return ret;
    }
    void WriteMsgToTxt(const char *str, FILE *fp){
        if(fp == NULL) return;
        fputs(str, fp);
        fclose(fp);
    }
    void SendMsgToClient(int fd, char *data, char flag){
        data[0] = flag;
        Send(fd, data, strlen(data), 0);
    }
    bool CompareUserPassword(){
        char s[30];
        fgets(s, 50, fp1);
        int cmpret = strcmp(s, (char*)msg.userpwd().c_str());
        return cmpret == 0;
    }
    bool GetUserLoginState(char *tagpath){
        fp2 = fopen(tagpath, "r");
        char tag[5];
        fgets(tag, 10, fp2);
        fclose(fp2);
        return strcmp(tag, "0") == 0;
    }
    void PositionInfo(char *pospath){
        fp3 = fopen(pospath, "r");
        for(int i = 0; i < 3; i++ ){
            char t[30];
            fgets(t, 50, fp3);
            t[strlen(t)-1] = '\0';
            double posnum = strtod(t, NULL);
            printf("%.f\n",posnum);
        }
        fclose(fp3);
    }
    void ListNowClientlist(){
        std::list<int>::iterator it = clientlist.begin();
        while(it != clientlist.end()){
            printf("%d ",*it);
            it++;
        }
        printf("\n");
    }
    void AddClientUser(int fd){
        clientlist.push_back(fd);
    }
    void CreateSocket(int fd){
        socketlist[fd] = msg.username();
    }
    void UserLogin(int fd, char *data, char *logpath, char *tagpath){
        if(fp1 == NULL){ // user not found
            SendMsgToClient(fd, data, '1');
        } else { // user exist
            // compare user's password
            if(CompareUserPassword()){ // password correct
                // get user's login state
                if(GetUserLoginState(tagpath)){ // login success
                    SendMsgToClient(fd, data, '2');
                    // change login mode
                    WriteMsgToTxt("1", fopen(tagpath, "w"));
                    // add now client to clientlist
                    AddClientUser(fd);
                    // create socket and add username to map
                    CreateSocket(fd);
                } else { // user already login
                    SendMsgToClient(fd, data, '3');
                }
            } else { // password wrong
                SendMsgToClient(fd, data, '4');
            }
        }
    }
    void AddEmptyMsg(int fd){
        Msg msgtmp;
        movelist[fd] = msgtmp;
    }
    void CreateUserMsg(int fd, char *data, char *logpath){
        fp2 = fopen(logpath, "a+");
        fputs((char*)msg.userpwd().c_str(), fp2);
        SendMsgToClient(fd, data, '2');
        fclose(fp2);
    }
    void CreatePosMsg(int fd, char *data, char *pospath){
        fp3 = fopen(pospath, "a+");
        for(int i = 0; i < 3; i++ ){
            fputs("0\n", fp3);
        }
        fclose(fp3);
    }
    void CreateLoginMsg(int fd, char *data, char *tagpath){
        fp4 = fopen(tagpath, "a+");
        fputs("0", fp4);
        fclose(fp4);
    }
    void CreateRotMsg(int fd, char *data, char *rotpath){
        fp5 = fopen(rotpath, "a+");
        for(int i = 0; i < 3; i++ ){
            fputs("0\n",fp5);
        }
        fclose(fp5);
    }
    void UserRegister(int fd, char *data, char *logpath, char *pospath, char *tagpath, char *rotpath){
        //printf("!!!\n");
        if(fp1 != NULL){ // user already exist
            SendMsgToClient(fd, data, '1');
        } else { // can register
            // add an empty msg
            AddEmptyMsg(fd);
            // create login_msg_txt to user_list
            CreateUserMsg(fd, data, logpath);
            // create pos_txt to user_pos
            CreatePosMsg(fd, data, pospath);
            // create login_state_txt to user_login
            CreateLoginMsg(fd, data, tagpath);
            // create rot_txt to user_rot
            CreateRotMsg(fd, data, rotpath);
        }
    }
    void UserLoad(int fd, char *data){
        printf("++++++++++++++++++++++++++++++++++  %d\n",loadlist.size());
        if(loadlist.find(fd) != loadlist.end()){
            return;
        }
        int sz = (int)clientlist.size();
        //printf("%d\n",sz);
        unsigned char head[4];
        IntToBytes(head, sz);
        //for(int i=0;i<4;i++) printf("%d ",head[i]); printf("\n");
        Send(fd, head, sizeof(head), 1);
        for(std::list<int>::iterator it = clientlist.begin(); it != clientlist.end(); it++){
            std::string nowuser = socketlist[(*it)];
            Msg msgtmp;
            msgtmp.set_username(nowuser);
            //printf("%s\n",(char*)nowuser.c_str());
            char *pospath = (char*)("/root/user_pos/" + nowuser + ".txt").c_str();
            FILE *fp;
            fp = fopen(pospath, "r");
            if(fp != NULL){
                for(int i = 0; i < 3; i++ ){
                    char t[20];
                    fgets(t, 30, fp);
                    if(i == 0) msgtmp.set_posx(strtod(t, NULL));
                    else if(i == 1) msgtmp.set_posy(strtod(t, NULL));
                    else if(i == 2) msgtmp.set_posz(strtod(t, NULL));
                }
                fclose(fp);
            } else {
                printf("user %s pos txt not found\n", nowuser.c_str());
            }
            char *rotpath = (char*)("/root/user_rot/" + nowuser + ".txt").c_str();
            fp = fopen(rotpath, "r");
            if(fp != NULL){
                for(int i = 0; i < 3; i++ ){
                    char t[20];
                    fgets(t, 30, fp);
                    if(i == 0) msgtmp.set_rotx(strtod(t, NULL));
                    else if(i == 1) msgtmp.set_roty(strtod(t, NULL));
                    else if(i == 2) msgtmp.set_rotz(strtod(t, NULL));
                }
                fclose(fp);
            } else {
                printf("user %s rot txt not fount\n", nowuser.c_str());
            }

            std::string s = msgtmp.SerializeAsString();

            IntToBytes(head, (int)s.size());
            std::string ss = "";
            for(int i = 0; i < 4; i++ ) ss += head[i];
            ss += s;
            //printf("%d %d\n", s.size(), ss.size());
            char *sss = (char*)ss.c_str();
            Send(fd, sss, (int)ss.size(), 2);
            //printf("%d %s\n", s.size(), sss);
        }
        if(gameStart == true){
            loadlist.insert(fd);
            printf("load fd %d to loadlist\n", fd);
        }
    }
    void UserMove(int fd, char *data){
        msg.set_username(socketlist[fd]);
        //printf("%.4f %.4f %.4f\n", msg.posx(), msg.posy(), msg.posz());
        movelist[fd] = msg; 
    }
    void SendMsgToClients(){
    
        unsigned char head[4];
        std::vector<std::string> vs;
        for(std::list<int>::iterator it = clientlist.begin(); it != clientlist.end(); it++){
            int now_fd = *it;
            Msg msgtmp = movelist[now_fd];
            printf("%d %.4f %.4f %.4f\n", now_fd, msgtmp.posx(), msgtmp.posy(), msgtmp.posz());
            msgtmp.set_frame(framenum);
            std::string s = msgtmp.SerializeAsString();
            IntToBytes(head, (int)s.size());
            std::string ss = "";
            for(int i = 0; i < 4; i++ ) ss += head[i];
            ss += s;
            vs.push_back(ss);
            /*char *sss = (char*)ss.c_str();
            ret = send(fd, sss, (int)ss.size(), 0);
            if(ret == -1){
                printf("send to socket %d error, errno = %d (%s)\n", fd, errno, strerror(errno));
            }*/
        }
        vvs.push_back(vs);
        int sz = (int)vs.size();
        IntToBytes(head, sz);
        for(std::list<int>::iterator it = clientlist.begin(); it != clientlist.end(); it++){
            int now_fd = *it;
            Send(now_fd, head, sizeof(head), 3);
            for(std::vector<std::string>::iterator itt = vs.begin(); itt != vs.end(); itt++){
                /*
                Msg msgtmptmp;
                msgtmptmp.ParseFromString(*itt);
                printf("%.4f %.4f %.4f\n", msgtmptmp.posx(), msgtmptmp.posy(), msgtmptmp.posz());
                */
                char *s = (char*)(*itt).c_str();
                Send(now_fd, s, (int)(*itt).size(), 4);
                /*if(ret == -1){
                    printf("send to socket %d error, errno = %d (%s)\n", now_fd, errno, strerror(errno));
                    // fxxk this user
                    CloseSocket(now_fd);
                    break;
                }*/
            }
        }
    }
    bool CloseSocket(int fd){
        printf("socket %d closed\n", fd);
        // delete socket_fd;
        clientlist.remove(fd);
        // get username
        std::map<int,std::string>::iterator it = socketlist.find(fd);
        if(it != socketlist.end()){
            WriteMsgToTxt("0", fopen((char*)("/root/user_login/" + socketlist[fd] + ".txt").c_str(), "w"));
            socketlist.erase(it);
        }
        int ret = close(fd);
        if(ret == -1){
            printf("close socket error, errno = %d (%s)\n", errno, strerror(errno));
            return false;
        }
        return true;
    }
    int BytesToInt(char *src){
        int ret = 0;
        ret |= (int)(src[0] & 0xff);
        ret |= (int)((src[1] & 0xff) << 8);
        ret |= (int)((src[2] & 0xff) << 16);
        ret |= (int)((src[3] & 0xff) << 24);
        return ret;
    }
    void IntToBytes(unsigned char *ret, int src){
        ret[0] = (unsigned char)(src & 0xff);
        ret[1] = (unsigned char)((src >> 8) & 0xff);
        ret[2] = (unsigned char)((src >> 16) & 0xff);
        ret[3] = (unsigned char)((src >> 24) & 0xff);
    }
    void Work(){
        while(true){
            //printf("%d\n",clientlist.size());
            if(gameStart == false && clientlist.size() == 2){
                gameStart = true;
                gettimeofday(&startTime, NULL);
                startTime.tv_sec += 5;
            } else if (gameStart == true && clientlist.size() == 0){
                break;
            }
            long long diff;
            int n = epoll_wait(epoll_fd, events, MAXEVENTS, 5);
            if(gameStart == true && loadlist.size() == 2){
                gettimeofday(&nowTime, NULL);
                diff = 1000000 * (nowTime.tv_sec - startTime.tv_sec) + (nowTime.tv_usec - startTime.tv_usec);
                //printf("diff = %lld\n", diff);
                if(diff >= 50000){
                    //printf("%lu\n", diff);
                    startTime.tv_usec += 50000;
                    SendMsgToClients();
                    framenum++;
                }
            }
            if(n == -1){
                printf("epoll wait error, errno = %d (%s)\n", errno, strerror(errno));
                break;
            }
            for(int i = 0; i < n; i++ ){
                int fd = events[i].data.fd;
                int fd_events = events[i].events;
                if((fd_events & EPOLLERR) ||
                   (fd_events & EPOLLHUP) ||
                   (!(fd_events & EPOLLIN))) {
                    printf("fd: %d error\n", fd);
                    CloseSocket(fd);
                    continue;
                } else if (fd == listen_fd){
                    struct sockaddr client_addr;
                    socklen_t client_addr_len = sizeof(client_addr);
                    int new_fd = accept(listen_fd, &client_addr, &client_addr_len);
                    if(new_fd == -1){
                        printf("accept socket error, errno = %d (%s)\n", errno, strerror(errno));
                        continue;
                    }
                    printf("new socket: %d\n", new_fd);
                    
                    // set nonblock
                    int flags = fcntl(new_fd, F_GETFL, 0);
                    fcntl(new_fd, F_SETFL, flags | O_NONBLOCK);
                    
                    event.data.fd = new_fd;
                    event.events = EPOLLIN;
                    int ret = epoll_ctl(epoll_fd, EPOLL_CTL_ADD, new_fd, &event);
                    if(ret == -1){
                        printf("epoll ctl error, errno = %d (%s)\n", errno, strerror(errno));
                        continue;
                    }
                } else {
                    char data[1024] = {0};
                    int ret = recv(fd, data + nowHeadLength, headLength - nowHeadLength, 0);
                    //printf("%d\n", ret);
                    if(ret > 0){
                        //printf("%d\n", ret);
                        nowHeadLength += ret;
                        if(nowHeadLength < headLength) continue;
                        nowHeadLength = 0;
                        // get data stream
                        GetDataStream(fd, data);

                        // init path
                        char *logpath, *pospath, *tagpath, *rotpath;
                        InitPath(&logpath, &pospath, &tagpath, &rotpath);
                        
                        fp1 = fopen(logpath, "r");
                        // user operation
                        memset(data, 0, 1024);
                        if(msg.optype() == 0){ // login
                            UserLogin(fd, data, logpath, tagpath);
                        } else if (msg.optype() == 1){ // register
                            UserRegister(fd, data, logpath, pospath, tagpath, rotpath);
                        } else if (msg.optype() == 2 && loadlist.size() != 2){ // load user
                            UserLoad(fd, data);
                        } else if (msg.optype() == 3 && loadlist.size() == 2 && diff >= 0){ // move user
                            UserMove(fd, data);
                        }
                        if(fp1 != NULL) fclose(fp1);
                    }
                    if(ret == 0){
                        // close socket
                        //printf("!!!\n");
                        if(!CloseSocket(fd)) continue;
                    } else if (ret == -1){
                        printf("recv error, errno = %d (%s)\n", errno, strerror(errno));
                    }
                }
            }
        }
    }
};


int main(int argc, char** argv){
    Server server;
    server.Init();
    if (argc > 1) server.SetPort(atoi(argv[1]));
    if (!server.SetServerSocket()) return 0;
    server.Work();
    server.Release();
    return 0;
}
