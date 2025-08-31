FROM eclipse-temurin:17-jdk-jammy

RUN useradd -m runner

# Download Gson library
RUN mkdir -p /home/runner/lib && \
    cd /home/runner/lib && \
    wget https://repo1.maven.org/maven2/com/google/code/gson/gson/2.10.1/gson-2.10.1.jar

WORKDIR /home/runner
COPY run.sh ./run.sh
COPY BaseDriver.txt ./BaseDriver.java
RUN chmod +x run.sh

USER runner
CMD ["./run.sh"]
