FROM eclipse-temurin:25-jdk

RUN useradd -m runner

# Download Gson library
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/* && \
    mkdir -p /home/runner/lib && \
    cd /home/runner/lib && \
    wget https://repo1.maven.org/maven2/com/google/code/gson/gson/2.13.2/gson-2.13.2.jar

WORKDIR /home/runner
COPY run.sh ./run.sh
COPY BaseDriver.txt ./BaseDriver.java
RUN chmod +x run.sh

USER runner
CMD ["./run.sh"]
