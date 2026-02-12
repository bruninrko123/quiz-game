export interface Question {
  id: number;

  text: string;

  options: string[];
}

export interface AnswerResponse {
  isCorrect: boolean;
}

export interface Score {
  totalQuestions: number;

  correctAnswers: number;
}

export interface AnswerRequest {
  QuestionId: number;

    SelectedOptionIndex: number;
    
    playerName: string;
}
