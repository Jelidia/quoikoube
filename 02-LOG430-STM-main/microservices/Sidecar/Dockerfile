FROM golang:1.20 as builder

WORKDIR /app
COPY main.go .  

RUN go mod init sidecar && go mod tidy  
RUN go get github.com/joho/godotenv  

# Désactivation de CGO pour la compatibilité avec Alpine
RUN go env -w CGO_ENABLED=0 GOOS=linux GOARCH=amd64  
RUN go build -o sidecar main.go  

FROM alpine:latest
WORKDIR /root/
COPY --from=builder /app/sidecar .  

# Rendre l'exécutable exécutable
RUN chmod +x /root/sidecar  

CMD ["./sidecar"]
