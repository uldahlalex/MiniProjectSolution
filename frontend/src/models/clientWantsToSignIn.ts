import {BaseDto} from "./baseDto";

export class ClientWantsToSignIn extends BaseDto<ClientWantsToSignIn> {
  email?: string;
  password?: string;
}
