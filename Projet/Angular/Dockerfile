FROM node:12.16.1-alpine

RUN npm install -g @angular/cli@11.2.12

WORKDIR /app

COPY ./package.json ./package-lock.json ./
RUN npm install

COPY . .

ENTRYPOINT ["npm", "run", "dockerSslProd"]