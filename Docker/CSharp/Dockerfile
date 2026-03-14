FROM mcr.microsoft.com/dotnet/sdk:10.0

RUN useradd -m runner
WORKDIR /home/runner

RUN dotnet new console -o App

COPY run.sh ./run.sh
COPY BaseDriver.txt ./App/BaseDriver.cs

# Pre-restore so runtime builds can use --no-restore
RUN cd App && dotnet restore
RUN chmod +x run.sh

USER runner
CMD ["./run.sh"]
