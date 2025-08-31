FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN useradd -m runner
WORKDIR /home/runner

RUN dotnet new console -o App

COPY run.sh ./run.sh
COPY BaseDriver.txt ./App/BaseDriver.cs
RUN chmod +x run.sh

USER runner
CMD ["./run.sh"]
