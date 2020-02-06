# This starts with a minimal Debian Buster image, and will work with a standalone built with the default runtime identifier.
ARG REPO=mcr.microsoft.com/dotnet/core/runtime-deps

FROM $REPO:3.1
RUN apt-get update
RUN apt-get -y install apt-utils
RUN apt-get -y install unzip wget
WORKDIR /tmp
RUN wget -O file.zip https://github.com/PaulBrack/Yamato/releases/download/v1.0.0-b/linux.binaries.zip && unzip file.zip && rm file.zip
RUN mkdir -p /yamato && mv release/publish/* /yamato && rm -r release
WORKDIR /yamato
RUN chmod +x Yamato.Console
ENV PATH=/yamato:$PATH
ENTRYPOINT /yamato/Yamato.Console
CMD "--help"
