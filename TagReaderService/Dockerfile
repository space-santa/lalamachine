from node:lts-alpine

RUN mkdir /app
WORKDIR /app
ENV NODE_ENV=production
COPY package.json package.json
COPY package-lock.json package-lock.json
RUN npm install
COPY src src

EXPOSE 3001
CMD [ "npm", "start" ]