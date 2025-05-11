FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN useradd -m runner
WORKDIR /home/runner
COPY run.sh ./run.sh
RUN chmod +x run.sh

USER runner
CMD ["./run.sh"]
